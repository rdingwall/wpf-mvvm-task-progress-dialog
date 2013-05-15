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

        public async Task ExecuteAsync(Func<Task> action, ProgressDialogOptions options)
        {
            if (action == null) throw new ArgumentNullException("action");
            await ExecuteAsyncInternal((token, progress) => action(), options);
        }

        public async Task ExecuteAsync(Func<CancellationToken, Task> action, ProgressDialogOptions options)
        {
            if (action == null) throw new ArgumentNullException("action");
            await ExecuteAsyncInternal((token, progress) => action(token), options);
        }

        public async Task ExecuteAsync(Func<IProgress<string>, Task> action, ProgressDialogOptions options)
        {
            if (action == null) throw new ArgumentNullException("action");
            await ExecuteAsyncInternal((token, progress) => action(progress), options,
                isCancellable: false);
        }

        public async Task ExecuteAsync(Func<CancellationToken, IProgress<string>, Task> action,
            ProgressDialogOptions options)
        {
            if (action == null) throw new ArgumentNullException("action");
            await ExecuteAsyncInternal(action, options);
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, ProgressDialogOptions options)
        {
            if (action == null) throw new ArgumentNullException("action");
            return await ExecuteAsyncInternal((token, progress) => action(), options);
        }

        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, ProgressDialogOptions options)
        {
            if (action == null) throw new ArgumentNullException("action");
            return await ExecuteAsyncInternal((token, progress) => action(token), options);
        }

        public async Task<T> ExecuteAsync<T>(Func<IProgress<string>, Task<T>> action, ProgressDialogOptions options)
        {
            if (action == null) throw new ArgumentNullException("action");
            return await ExecuteAsyncInternal((token, progress) => action(progress), options,
                isCancellable: false);
        }

        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, IProgress<string>, Task<T>> action,
            ProgressDialogOptions options)
        {
            if (action == null) throw new ArgumentNullException("action");
            return await ExecuteAsyncInternal(action, options);
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

        async private Task<T> ExecuteAsyncInternal<T>(
            Func<CancellationToken, IProgress<string>, Task<T>> action,
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

                Task<T> task = action(cancellationToken, viewModel.Progress);

                task.ContinueWith(_ => viewModel.Close = true);

                window.ShowDialog();

                return await task;
            }
        }

        async private Task ExecuteAsyncInternal(
            Func<CancellationToken, IProgress<string>, Task> action,
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

                Task task = action(cancellationToken, viewModel.Progress);

                task.ContinueWith(_ => viewModel.Close = true);

                window.ShowDialog();

                await task;
            }
        }
    }
}