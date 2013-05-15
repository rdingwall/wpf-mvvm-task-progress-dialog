using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            Task<int> task = dialogService.ExecuteAsync(DoWork, new ProgressDialogOptions { WindowTitle = "Loading files" });

            MessageBox.Show(String.Format("Result = {0}", task.Result));
        }

        static async Task<int> DoWork(CancellationToken cancellationToken, IProgress<string> progress)
        {
            return await Task.Factory.StartNew(() => progress.Report("First"), cancellationToken)
                             .ContinueWith(_ => Thread.Sleep(1000), cancellationToken)
                             .ContinueWith(_ => progress.Report("Second"), cancellationToken)
                             .ContinueWith(_ => Thread.Sleep(1000), cancellationToken)
                             .ContinueWith(_ => 42);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}