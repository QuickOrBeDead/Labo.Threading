namespace Labo.Threading.Tests
{
    using System;
    using System.Diagnostics;

    public static class Wait
    {
        public static bool While(Func<bool> conditionFunc, int timeoutInMilliseconds)
        {
            Lazy<Stopwatch> stopWatchInitializer = new Lazy<Stopwatch>(Stopwatch.StartNew, true);
            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (conditionFunc())
            {
                if (stopWatchInitializer.Value.ElapsedMilliseconds >= timeoutInMilliseconds)
                {
                    stopWatchInitializer.Value.Stop();
                    return false;
                }
            }

            stopWatchInitializer.Value.Stop();
            return true;
        }
    }
}
