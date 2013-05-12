using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProgressDialogEx.ProgressDialog
{
    public class ProgressDialogService : IProgressDialogService
    {
        readonly TaskFactory taskFactory;

        public ProgressDialogService() : this(Task.Factory) { }

        public ProgressDialogService(TaskFactory taskFactory)
        {
            if (taskFactory == null) throw new ArgumentNullException("taskFactory");
            this.taskFactory = taskFactory;
        }

        public void Execute(Action action, ProgressDialogOptions options)
        {
            if (action == null) throw new ArgumentNullException("action");
            ExecuteInternal((token, progress) => action(), options,
                isCancellable: false);
        }

        public void Execute(Action<CancellationToken> action, ProgressDialogOptions options)
        {
            if (action == null) throw new ArgumentNullException("action");
            ExecuteInternal((token, progress) => action(token), options);
        }

        public void Execute(Action<IProgress<string>> action, ProgressDialogOptions options)
        {
            if (action == null) throw new ArgumentNullException("action");
            ExecuteInternal((token, progress) => action(progress), options);
        }

        public void Execute(
            Action<CancellationToken, IProgress<string>> action, ProgressDialogOptions options)
        {
            if (action == null) throw new ArgumentNullException("action");
            ExecuteInternal(action, options);
        }

        public bool TryExecute<T>(Func<T> action, ProgressDialogOptions options, out T result)
        {
            if (action == null) throw new ArgumentNullException("action");
            return TryExecuteInternal((token, progress) => action(), options, out result,
                isCancellable: false);
        }

        public bool TryExecute<T>(Func<CancellationToken, T> action, ProgressDialogOptions options, out T result)
        {
            if (action == null) throw new ArgumentNullException("action");
            return TryExecuteInternal((token, progress) => action(token), options, out result);
        }

        public bool TryExecute<T>(Func<IProgress<string>, T> action, ProgressDialogOptions options, out T result)
        {
            if (action == null) throw new ArgumentNullException("action");
            return TryExecuteInternal((token, progress) => action(progress), options, out result,
                isCancellable: false);
        }

        public bool TryExecute<T>(Func<CancellationToken, IProgress<string>, T> action, 
            ProgressDialogOptions options, out T result)
        {
            if (action == null) throw new ArgumentNullException("action");
            return TryExecuteInternal(action, options, out result);
        }

        private void ExecuteInternal(Action<CancellationToken, IProgress<string>> action, 
            ProgressDialogOptions options, bool isCancellable = true)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (options == null) throw new ArgumentNullException("options");

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                CancellationToken cancellationToken = cancellationTokenSource.Token;

                var cancelCommand = isCancellable ? new CancelCommand(cancellationTokenSource) : null;

                var viewModel = new ProgressDialogWindowViewModel(
                    options, cancellationToken, cancelCommand);

                var window = new ProgressDialogWindow
                                 {
                                     DataContext = viewModel
                                 };

                var task = taskFactory
                    .StartNew(() => action(cancellationToken, viewModel.Progress),
                              cancellationToken);

                task.ContinueWith(_ => viewModel.Close = true);

                window.ShowDialog();
            }
        }

        private bool TryExecuteInternal<T>(
            Func<CancellationToken, IProgress<string>, T> action, 
            ProgressDialogOptions options, out T result, bool isCancellable = true)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (options == null) throw new ArgumentNullException("options");

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                CancellationToken cancellationToken = cancellationTokenSource.Token;

                var cancelCommand = isCancellable ? new CancelCommand(cancellationTokenSource) : null;
                
                var viewModel = new ProgressDialogWindowViewModel(
                    options, cancellationToken, cancelCommand);

                var window = new ProgressDialogWindow
                {
                    DataContext = viewModel
                };

                var task = taskFactory
                    .StartNew(() => action(cancellationToken, viewModel.Progress),
                              cancellationToken);

                task.ContinueWith(_ => viewModel.Close = true);

                window.ShowDialog();

                if (task.IsCanceled)
                {
                    result = default(T);
                    return false;
                }

                if (task.IsCompleted)
                {
                    result = task.Result;
                    return true;
                }

                result = default(T);
                return false;
            }
        }
    }
}