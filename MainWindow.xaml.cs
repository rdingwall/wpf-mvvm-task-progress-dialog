using System;
using System.Threading;
using System.Windows;
using ProgressDialogEx.ProgressDialog;

namespace ProgressDialogEx
{
    public partial class MainWindow : Window
    {
        readonly IProgressDialogService progressDialog = new ProgressDialogService();

        readonly ProgressDialogOptions options = new ProgressDialogOptions
                                                     {
                                                         WindowTitle = "Progress Dialog",
                                                         Label = "Progress Dialog Label"
                                                     };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            MyLongRunningCommand = new MyLongRunningCommand(progressDialog);
        }

        public MyLongRunningCommand MyLongRunningCommand { get; private set; }

        // Using non-MVVM code-behind for purposes of example.

        void ShowProgressDialog(object sender, RoutedEventArgs e)
        {
            progressDialog.Execute(Sleep, options);
        }

        void ShowProgressDialogWithCancelButton(object sender, RoutedEventArgs e)
        {
            progressDialog.Execute(CancellableSleep, options);
        }

        void ShowProgressDialogWithCancelButtonAndProgress(object sender, RoutedEventArgs e)
        {
            progressDialog.Execute(CancellableSleepWithProgress, options);
        }

        void ShowProgressDialogWithResult(object sender, RoutedEventArgs e)
        {
            int result;
            if (progressDialog.TryExecute(SleepAndReturnResult, options, out result))
                MessageBox.Show(String.Format("Result = {0}", result));
        }

        void ShowProgressDialogWithResultAndCancelButton(object sender, RoutedEventArgs e)
        {
            int result;
            if (progressDialog.TryExecute(CancellableSleepAndReturnResult, options, out result))
                MessageBox.Show(String.Format("Result = {0}", result));
        }

        void ShowProgressDialogWithResultAndCancelButtonAndProgress(object sender, RoutedEventArgs e)
        {
            int result;
            if (progressDialog.TryExecute(CancellableSleepWithProgressAndReturnResult, options, out result))
                MessageBox.Show(String.Format("Result = {0}", result));
        }

        void ShowProgressDialogWithError(object sender, RoutedEventArgs e)
        {
            try
            {
                int result;
                if (progressDialog.TryExecute(SleepAndThrowError, options, out result))
                    MessageBox.Show(String.Format("Result = {0}", result));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void ShowProgressDialogWithErrorAndCancelButton(object sender, RoutedEventArgs e)
        {
            try
            {
                int result;
                if (progressDialog.TryExecute(CancellableSleepAndThrowError, options, out result))
                    MessageBox.Show(String.Format("Result = {0}", result));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void ShowProgressDialogWithErrorAndCancelButtonAndProgress(object sender, RoutedEventArgs e)
        {
            try
            {
                int result;
                if (progressDialog.TryExecute(CancellableSleepWithProgressAndThrowError, options, out result))
                    MessageBox.Show(String.Format("Result = {0}", result));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region Sleeps

        const int Limit = 20;

        static void CancellableSleep(CancellationToken cancellationtoken)
        {
            for (int i = 0; i < Limit; i++)
            {
                cancellationtoken.ThrowIfCancellationRequested();
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        static void CancellableSleepWithProgress(CancellationToken cancellationtoken, 
            IProgress<string> progress)
        {
            for (int i = 0; i < Limit; i++)
            {
                cancellationtoken.ThrowIfCancellationRequested();
                progress.Report(String.Format("Processing {0}/{1}", i, Limit));
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        static void Sleep()
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        static int SleepAndReturnResult()
        {
            Sleep();
            return 42;
        }

        static int CancellableSleepAndReturnResult(CancellationToken cancellationtoken)
        {
            CancellableSleep(cancellationtoken);
            return 42;
        }

        static int CancellableSleepWithProgressAndReturnResult(CancellationToken cancellationtoken, IProgress<string> progress)
        {
            CancellableSleepWithProgress(cancellationtoken, progress);
            return 42;
        }

        static int CancellableSleepWithProgressAndThrowError(CancellationToken cancellationtoken, IProgress<string> progress)
        {
            CancellableSleepWithProgress(cancellationtoken, progress);
            throw new Exception("Test exception");
        }

        static int CancellableSleepAndThrowError(CancellationToken cancellationtoken)
        {
            CancellableSleep(cancellationtoken);
            throw new Exception("Test exception");
        }

        static int SleepAndThrowError()
        {
            Sleep();
            throw new Exception("Test exception");
        }

        #endregion
    }
}
