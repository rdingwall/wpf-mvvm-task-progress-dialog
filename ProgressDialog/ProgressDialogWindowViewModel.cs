using System;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;

namespace ProgressDialogEx.ProgressDialog
{
    public class ProgressDialogWindowViewModel : ViewModelBase
    {
        string windowTitle;
        string label;
        string subLabel;
        bool close;

        public string WindowTitle
        {
            get { return windowTitle; }
            private set
            {
                windowTitle = value;
                RaisePropertyChanged(() => WindowTitle);
            }
        }

        public string Label
        {
            get { return label; }
            private set
            {
                label = value;
                RaisePropertyChanged(() => Label);
            }
        }

        public string SubLabel
        {
            get { return subLabel; }
            private set
            {
                subLabel = value;
                RaisePropertyChanged(() => SubLabel);
            }
        }

        public bool Close
        {
            get { return close; }
            set
            {
                close = value;
                RaisePropertyChanged(() => Close);
            }
        }

        public bool IsCancellable { get { return CancelCommand != null; } }

        public CancelCommand CancelCommand { get; private set; }
        public IProgress<string> Progress { get; private set; }

        public ProgressDialogWindowViewModel(
            ProgressDialogOptions options,
            CancellationToken cancellationToken,
            CancelCommand cancelCommand)
        {
            if (options == null) throw new ArgumentNullException("options");
            WindowTitle = options.WindowTitle;
            Label = options.Label;
            CancelCommand = cancelCommand; // can be null (not cancellable)
            cancellationToken.Register(OnCancelled);
            Progress = new Progress<string>(OnProgress);
        }

        void OnCancelled()
        {
            // Cancellation may come from a background thread.
            if (DispatcherHelper.UIDispatcher != null)
                DispatcherHelper.CheckBeginInvokeOnUI(() => Close = true);
            else
                Close = true;
        }

        void OnProgress(string obj)
        {
            // Progress will probably come from a background thread.
            if (DispatcherHelper.UIDispatcher != null)
                DispatcherHelper.CheckBeginInvokeOnUI(() => SubLabel = obj);
            else
                SubLabel = obj;
        }
    }
}