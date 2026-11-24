using System;
using System.Threading.Tasks;

namespace Elements.Core;

/// <summary>
/// Contains utility methods for throttling/rate-limiting operations
/// </summary>
public static class ThrottleHelper
{
    /// <summary>
    /// Runs an operation repeatedly until it succeeds or a maximum number of attempts is reached.
    /// A delay is inserted between each attempt, doubling each time, up to a maximum delay.
    /// </summary>
    /// <param name="func">Function to run for each attempt, returning whether the operation was successful</param>
    /// <param name="baseDelayMs"></param>
    /// <param name="maxDelayMs"></param>
    /// <param name="maxAttempts"></param>
    /// <returns></returns>
    public static async Task<bool> RetryWithBackoff(Func<Task<bool>> func, int baseDelayMs = 250, int maxDelayMs = 5000, int maxAttempts = 5)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxAttempts);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(baseDelayMs);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxDelayMs);

        int attempts = 0;
        int delayMs = baseDelayMs;

        while (true)
        {
            // Attempt the operation
            if (await func())
                return true;

            // Finish if we've reached the max attempts
            attempts++;
            if (attempts >= maxAttempts)
                return false;

            // Delay the next attempt, doubling the duration of the delay each attempt (up to the max delay)
            delayMs *= 2;
            await Task.Delay(Math.Min(delayMs, maxDelayMs));
        }
    }

    /// <summary>
    /// Runs an operation repeatedly until it succeeds or a maximum number of attempts is reached.
    /// A delay is inserted between each attempt, doubling each time, up to a maximum delay.
    /// </summary>
    /// <param name="func">Function to run for each attempt, returning whether the operation was successful</param>
    /// <param name="baseDelay"></param>
    /// <param name="maxDelay"></param>
    /// <param name="maxAttempts"></param>
    /// <returns></returns>
    public static async Task<bool> RetryWithBackoff(Func<Task<bool>> func, TimeSpan baseDelay, TimeSpan maxDelay, int maxAttempts = 5)
    {
        return await RetryWithBackoff(
            func,
            maxAttempts,
            int.CreateChecked(baseDelay.TotalMilliseconds),
            int.CreateChecked(maxDelay.TotalMilliseconds)
        );
    }
}
