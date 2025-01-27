namespace Core.ToolKit.Utilities;

public static class BackgroundWorkerAlert
{
    public static async Task RunWithProgressAsync<T>(Func<IProgress<T>, Task<T>> work, Action<T>? onProgress = null, Action<T>? onSuccess = null, Action<Exception>? onError = null, CancellationToken cancellationToken = default)
    {
        if (work == null) throw new ArgumentNullException(nameof(work));

        var progressReporter = new Progress<T>(value => onProgress?.Invoke(value));

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await work(progressReporter);
            onSuccess?.Invoke(result);
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
        }
    }
}