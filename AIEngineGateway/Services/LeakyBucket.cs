namespace AIEngineGateway.Services
{
    /// <summary>
    /// THIS IS THE CUSTOM RATE LIMIT ALGORITHM 
    /// (LEAKY BUCKET) IMPLEMENTED BY {{ GANESH VEERAVALLI }} IT WAS
    /// VERY SENSITIVE BE ATTENTION WHILE CHANGING ANY 
    /// CORE LOGIC.
    /// </summary>
    public class LeakyBucket
    {
        private static int _capacity;
        private static Queue<Tokens> _leakyBucket;
        private static int _refillInSeconds;
        public static void InitializeBucket(int capacity, int refillInSeconds)
        {
            _capacity = capacity;
            _refillInSeconds = refillInSeconds;
            _leakyBucket = new Queue<Tokens>();
        }

        private static readonly Object _lock = new object();
        public static void StartLeakProcessor()
        {
            Thread leakprocessor = new Thread(() =>
            {
                while (true)
                {
                    lock (_lock)
                    {
                        LeakRequest();
                    }
                    Thread.Sleep(_refillInSeconds * 1000);
                }
            });
            leakprocessor.IsBackground = true;
            leakprocessor.Start();
        }

        public static void LeakRequest()
        {
            if (_leakyBucket.Count > 0)
            {
                _leakyBucket.Dequeue();
            }
        }

        public static Boolean EnqueueRequest(string token)
        {
            lock (_lock)
            {
                if (_leakyBucket.Count >= _capacity)
                    return false;
                _leakyBucket.Enqueue(new Tokens
                {
                    ClientId = token,
                    TimeStamp = DateTime.UtcNow
                });
                return true;
            }
        }

        class Tokens
        {
            public string ClientId { get; set; }
            public DateTime TimeStamp { get; set; }
        }
    }
}
