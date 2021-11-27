﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Panoptes.Model.Messages;
using Panoptes.Model.Settings;
using System.Threading.Tasks;

namespace Panoptes.ViewModels
{
    public abstract class DocumentPaneViewModel : PaneViewModel
    {
        private bool _canClose;
        private string _key;

        public DocumentPaneViewModel(IMessenger messenger, ISettingsManager settingsManager)
            : base(messenger, settingsManager)
        { }

        public bool CanClose
        {
            get { return _canClose; }
            set
            {
                if (_canClose == value) return;
                _canClose = value;
                OnPropertyChanged();
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }
    }

    public abstract class ToolPaneViewModel : PaneViewModel
    {
        private bool _isVisible = true;

        private string _name;

        public ToolPaneViewModel(IMessenger messenger, ISettingsManager settingsManager)
            : base(messenger, settingsManager)
        { }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// View model for a docking pane
    /// </summary>
    public abstract class PaneViewModel : ObservableRecipient
    {
        private bool _isSelected;
        private bool _isActive;

        public ISettingsManager SettingsManager { get; }

        public PaneViewModel(IMessenger messenger, ISettingsManager settingsManager)
            : base(messenger)
        {
            SettingsManager = settingsManager;
            Messenger.Register<PaneViewModel, SettingsMessage>(this, async (r, m) => await r.UpdateSettingsAsync(m.Value, m.Type).ConfigureAwait(false));
        }

        protected abstract Task UpdateSettingsAsync(UserSettings userSettings, UserSettingsUpdate type);

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;

                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                OnPropertyChanged();
            }
        }
    }
}
