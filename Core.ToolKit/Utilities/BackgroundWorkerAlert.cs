namespace Core.ToolKit.Utilities;

public static class BackgroundWorkerAlert
{
    /// <summary>
    /// Executes a background task with advanced progress tracking and detailed error handling.
    /// </summary>
    /// <typeparam name="T">Type of the result produced by the task.</typeparam>
    /// <param name="work">The task to execute.</param>
    /// <param name="onProgress">Action to report progress updates.</param>
    /// <param name="onSuccess">Action to execute on successful completion.</param>
    /// <param name="onError">Action to execute on error.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task RunWithProgressAsync<T>(Func<IProgress<int>, Task<T>> work, Action<int>? onProgress = null, Action<T>? onSuccess = null, Action<Exception>? onError = null)
    {
        if (work == null) throw new ArgumentNullException(nameof(work));

        var progressReporter = new Progress<int>(value => onProgress?.Invoke(value));

        try
        {
            var result = await work(progressReporter);
            onSuccess?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
        }
    }
}