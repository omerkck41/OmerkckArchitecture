﻿using Core.CrossCuttingConcerns.GlobalException.Exceptions;

namespace Core.Infrastructure.UniversalFTP.Helper;

public static class RetryHelper
{
    public static async Task<T> RetryAsync<T>(Func<Task<T>> operation, int retryCount, TimeSpan delay)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                return await operation();
            }
            catch
            {
                if (i == retryCount - 1)
                    throw;
                await Task.Delay(delay);
            }
        }
        throw new CustomInvalidOperationException("Retry mechanism failed.");
    }
}