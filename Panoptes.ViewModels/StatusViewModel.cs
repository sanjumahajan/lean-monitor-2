﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Panoptes.Model;
using Panoptes.Model.Messages;
using Panoptes.Model.Sessions;
using QuantConnect;
using System.Collections.Generic;
using System.Linq;

namespace Panoptes.ViewModels
{
    public sealed class StatusViewModel : ObservableRecipient
    {
        private readonly IMessenger _messenger;
        private readonly ISessionService _sessionService;

        public StatusViewModel(IMessenger messenger, ISessionService sessionService) : base(messenger)
        {
            _messenger = messenger;
            _sessionService = sessionService;

            _messenger.Register<StatusViewModel, SessionOpenedMessage>(this, (r, _) =>
            {
                r.Progress = 0;
                r.IsProgressIndeterminate = false;
                r.OnPropertyChanged(nameof(IsSessionActive));
            });

            _messenger.Register<StatusViewModel, SessionClosedMessage>(this, (r, _) =>
            {
                r.SessionName = string.Empty;
                r.ProjectName = string.Empty;
                r.SessionState = SessionState.Unsubscribed;
                r.OnPropertyChanged(nameof(IsSessionActive));
            });

            _messenger.Register<StatusViewModel, SessionStateChangedMessage>(this, (r, m) => r.SessionState = m.State);

            _messenger.Register<StatusViewModel, SessionUpdateMessage>(this, (r, m) =>
            {
                r.Progress = m.ResultContext.Progress;
                r.SessionName = m.ResultContext.Name;
                r.ProjectName = m.ResultContext.Project;

                switch (m.ResultContext.Result.ResultType)
                {
                    case ResultType.Backtest:
                        r.IsLive = false;
                        r.IsProgressIndeterminate = false;
                        break;

                    case ResultType.Live:
                        r.IsLive = true;
                        r.IsProgressIndeterminate = true;
                        break;
                }

                ProcessServerStatistics(m.ResultContext.Result.ServerStatistics);
            });

            _messenger.Register<StatusViewModel, AlgorithmStatusMessage>(this, (r, m) => r.AlgorithmStatus = m.Value.Status);
        }

        private string _serverStatistics;
        public string ServerStatistics
        {
            get { return _serverStatistics; }
            set
            {
                if (_serverStatistics == value) return;
                _serverStatistics = value;
                OnPropertyChanged();
            }
        }

        private bool? _isLive;
        public bool? IsLive
        {
            get { return _isLive; }
            set
            {
                if (_isLive == value) return;
                _isLive = value;
                OnPropertyChanged();
            }
        }

        private decimal _progress;
        public decimal Progress
        {
            get { return _progress; }
            set
            {
                if (_progress == value) return;
                _progress = value;
                OnPropertyChanged();
            }
        }

        private string _sessionName;
        public string SessionName
        {
            get { return _sessionName; }
            set
            {
                if (_sessionName == value) return;
                _sessionName = value;
                OnPropertyChanged();
            }
        }

        private string _projectName;
        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                if (_projectName == value) return;
                _projectName = value;
                OnPropertyChanged();
            }
        }

        private bool _isProgressIndeterminate;
        public bool IsProgressIndeterminate
        {
            get { return _isProgressIndeterminate; }
            set
            {
                if (_isProgressIndeterminate == value) return;
                _isProgressIndeterminate = value;
                OnPropertyChanged();
            }
        }

        private SessionState _sessionState = SessionState.Unsubscribed;
        public SessionState SessionState
        {
            get { return _sessionState; }
            set
            {
                if (_sessionState == value) return;
                _sessionState = value;
                OnPropertyChanged();
            }
        }

        private AlgorithmStatus? _algorithmStatus;
        public AlgorithmStatus? AlgorithmStatus
        {
            get { return _algorithmStatus; }
            set
            {
                if (_algorithmStatus == value) return;
                _algorithmStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsSessionActive => _sessionService.IsSessionActive;

        private void ProcessServerStatistics(IDictionary<string, string> serverStatistics)
        {
            if (serverStatistics == null || serverStatistics.Count == 0) return;
            ServerStatistics = $"Server: [{string.Join(", ", serverStatistics.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}: {kvp.Value}"))}]";
        }
    }
}
