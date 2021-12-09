using System;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;

namespace andywiecko.BurstCollections
{
    internal static class InternalUtility
    {
        public static Id AsId<Id>(int value)
            where Id : unmanaged, IIndexer
        {
            CheckIdSize<Id>();
            return UnsafeUtility.As<int, Id>(ref value);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        unsafe private static void CheckIdSize<Id>() where Id : unmanaged, IIndexer
        {
            if (sizeof(Id) != sizeof(int))
            {
                throw new ArgumentException($"{typeof(Id).Name} does not match `int` size.");
            }
        }
    }
}