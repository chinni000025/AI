namespace AIEngineCore.Extensions
{
    public static class EngineExponentialBackOff
    {
        public static TimeSpan GetExponentialBackoff(this int retryCount, double baseDelaySeconds = 1, double maxDelaySeconds = 32)
        {
            retryCount = Math.Max(1, retryCount);
            double exponentialDelay = Math.Min(baseDelaySeconds * Math.Pow(2, retryCount - 1), maxDelaySeconds);
            double halfDelay = exponentialDelay / 2;
            double delayWithJitter = halfDelay + Random.Shared.NextDouble() * halfDelay;
            return TimeSpan.FromSeconds(delayWithJitter);
        }
    }
}