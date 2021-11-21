﻿using Microsoft.Toolkit.Mvvm.Messaging;
using Panoptes.Model.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Panoptes.ViewModels.Panels
{
    public sealed class LogPanelViewModel : ToolPaneViewModel
    {
        private enum ActionsThreadUI : byte
        {
            /// <summary>
            /// Add log entry.
            /// </summary>
            LogEntryAdd = 0,

            /// <summary>
            /// Clear observable collections.
            /// </summary>
            Clear = 1,
        }

        private readonly BackgroundWorker _resultBgWorker;

        private readonly BlockingCollection<LogEntryReceivedMessage> _resultsQueue = new BlockingCollection<LogEntryReceivedMessage>();

        private ObservableCollection<LogPanelItemViewModel> _logEntries = new ObservableCollection<LogPanelItemViewModel>();
        public ObservableCollection<LogPanelItemViewModel> LogEntries
        {
            get { return _logEntries; }
            set
            {
                _logEntries = value;
                OnPropertyChanged();
            }
        }

        public LogPanelViewModel(IMessenger messenger)
            : base(messenger)
        {
            Name = "Log";
            Messenger.Register<LogPanelViewModel, LogEntryReceivedMessage>(this, (r, m) => r._resultsQueue.Add(m));
            Messenger.Register<LogPanelViewModel, SessionClosedMessage>(this, (r, _) => r.Clear());

            _resultBgWorker = new BackgroundWorker() { WorkerSupportsCancellation = true, WorkerReportsProgress = true };
            _resultBgWorker.DoWork += ResultQueueReader;
            _resultBgWorker.ProgressChanged += (s, e) =>
            {
                switch ((ActionsThreadUI)e.ProgressPercentage)
                {
                    case ActionsThreadUI.LogEntryAdd:
                        if (e.UserState is not LogPanelItemViewModel lpivm)
                        {
                            throw new ArgumentException($"LogPanelViewModel: Expecting {nameof(e.UserState)} of type 'LogPanelItemViewModel' but received '{e.UserState.GetType()}'", nameof(e));
                        }
                        LogEntries.Add(lpivm);
                        break;

                    case ActionsThreadUI.Clear:
                        LogEntries.Clear();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(e), "LogPanelViewModel: Unknown 'ProgressPercentage' passed.");
                }
            };

            _resultBgWorker.RunWorkerCompleted += (s, e) => { /*do anything here*/ };
            _resultBgWorker.RunWorkerAsync();
        }

        private void Clear()
        {
            try
            {
                Debug.WriteLine("LogPanelViewModel: Clear");
                _resultBgWorker.ReportProgress((int)ActionsThreadUI.Clear);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LogPanelViewModel: ERROR\n{ex}");
                throw;
            }
        }

        private void ResultQueueReader(object sender, DoWorkEventArgs e)
        {
            while (!_resultBgWorker.CancellationPending)
            {
                var logEntryMessage = _resultsQueue.Take(); // Need cancelation token
                _resultBgWorker.ReportProgress((int)ActionsThreadUI.LogEntryAdd, new LogPanelItemViewModel
                {
                    DateTime = logEntryMessage.DateTime,
                    Message = logEntryMessage.Message,
                    EntryType = logEntryMessage.EntryType
                });
            }
        }
    }
}
