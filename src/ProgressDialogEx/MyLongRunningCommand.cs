using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ProgressDialogEx.ProgressDialog;

namespace ProgressDialogEx
{
    public class MyLongRunningCommand : ICommand
    {
        readonly IProgressDialogService dialogService;

        public MyLongRunningCommand(IProgressDialogService dialogService)
        {
            this.dialogService = dialogService;
        }

        public void Execute(object parameter)
        {
            dialogService.Execute(DoWork, new ProgressDialogOptions { WindowTitle = "Loading files" });
        }

        void DoWork(CancellationToken cancellationToken, IProgress<string> progress)
        {
            Task.Factory.StartNew(() => progress.Report("First"), cancellationToken)
                .ContinueWith(_ => Thread.Sleep(1000), cancellationToken)
                .ContinueWith(_ => progress.Report("Second"), cancellationToken)
                .ContinueWith(_ => Thread.Sleep(1000), cancellationToken)
                .Wait();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}