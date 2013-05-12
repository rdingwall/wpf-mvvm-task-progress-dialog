using System;
using System.Threading;
using System.Windows.Input;

namespace ProgressDialogEx.ProgressDialog
{
    public class CancelCommand : ICommand
    {
        readonly CancellationTokenSource cancellationTokenSource;

        public event EventHandler CanExecuteChanged;

        public CancelCommand(CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource == null) throw new ArgumentNullException("cancellationTokenSource");
            this.cancellationTokenSource = cancellationTokenSource;
        }

        public bool CanExecute(object parameter)
        {
            return !cancellationTokenSource.IsCancellationRequested;
        }

        public void Execute(object parameter)
        {
            cancellationTokenSource.Cancel();

            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);

            CommandManager.InvalidateRequerySuggested();
        }
    }
}