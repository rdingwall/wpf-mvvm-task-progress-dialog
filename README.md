WPF MVVM Progress Dialog Example
=============================

This is a quick example WPF application that shows an MVVM-style progress meter dialog, using the .NET 4.5 Task Parallel Library (TPL):

* An injectable, stateless IProgressDialogService, that can be injected for unit testing purposes. 
* A progress meter using .NET 4 Tasks (not BackgroundWorkers).
* Cancelling of Tasks via CancellationTokens.
* Exception handling back to the original thread.
* Reporting background progress updates via IProgress&lt;T&gt;.
* Tasks can either be declared as void, or have a return value.
* Using an attached property to hide [the close button in the progress dialog's title bar](http://stackoverflow.com/questions/743906/how-to-hide-close-button-in-wpf-window) using Win32 interop.
* Using an attached property to implement a [close window command using MVVM](http://stackoverflow.com/questions/11945821/implementing-close-window-command-with-mvvm/).

Note it uses [MVVM light](http://mvvmlight.codeplex.com) for some INotifyPropertyChanged and DispatcherHelper stuff.

### Example Usage
The following example shows how you might set up a simple progress dialog, that can be cancelled and reports progress.

```csharp
public class MyLongRunningCommand : ICommand
{
    IProgressDialogService dialogService;

    public void Execute(object parameter)
    {
        dialogService.Execute(DoWork, new ProgressDialogOptions { WindowTitle = "Loading files" });
    }

    void DoWork(CancellationToken cancellationToken, IProgress<string> progress)
    {
        for (int i = 0; i < 100; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            progress.Report(String.Format("Processing task {0}", i));

            // Do some work
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }

    ...
}
```

### Example Usage - Composing Multiple Tasks
The following example shows how you might set up a progress dialog that is composed of multiple tasks chained together (that are cancellable and each report progress).

```csharp
public class MyLongRunningCommand : ICommand
{
    readonly IProgressDialogService dialogService;

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

    ...
}
```

### IProgressDialogService
The following signatures are supported - Execute (no return value) and TryExecute (a return value is expected, but will return false if cancelled).
```csharp
public interface IProgressDialogService
{
    /// <summary>
    /// Executes a non-cancellable task.
    /// </summary>
    void Execute(Action action, ProgressDialogOptions options);

    /// <summary>
    /// Executes a cancellable task.
    /// </summary>
    void Execute(Action<CancellationToken> action, ProgressDialogOptions options);

    /// <summary>
    /// Executes a non-cancellable task that reports progress.
    /// </summary>
    void Execute(Action<IProgress<string>> action, ProgressDialogOptions options);

    /// <summary>
    /// Executes a cancellable task that reports progress.
    /// </summary>
    void Execute(Action<CancellationToken, IProgress<string>> action, ProgressDialogOptions options);

    /// <summary>
    /// Executes a non-cancellable task, that returns a value.
    /// </summary>
    bool TryExecute<T>(Func<T> action, ProgressDialogOptions options, out T result);

    /// <summary>
    /// Executes a cancellable task that returns a value.
    /// </summary>
    bool TryExecute<T>(Func<CancellationToken, T> action, ProgressDialogOptions options, out T result);

    /// <summary>
    /// Executes a non-cancellable task that reports progress and returns a value.
    /// </summary>
    bool TryExecute<T>(Func<IProgress<string>, T> action, ProgressDialogOptions options, out T result);

    /// <summary>
    /// Executes a cancellable task that reports progress and returns a value.
    /// </summary>
    bool TryExecute<T>(Func<CancellationToken, IProgress<string>, T> action, ProgressDialogOptions options, out T result);
}
```

### Credits

This is based upon Jürgen Bäurle's original blog post here: http://www.parago.de/2011/04/how-to-implement-a-modern-progress-dialog-for-wpf-applications/
