using System;
using System.Threading;

namespace EarlMini.Core
{
    internal static class SafeRandomProvider
    {
        private static int _seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> RandomWrapper = new ThreadLocal<Random>( () => new Random( Interlocked.Increment( ref _seed ) ) );

        /// <summary>
        /// Provides a reliable thread-safe 'Random' class 
        /// </summary>
        /// <returns></returns>
        public static Random GetThreadRandom()
        {
            return RandomWrapper.Value;
        }
    }
}
