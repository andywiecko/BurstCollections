using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace andywiecko.BurstCollections
{
    public static class UnityCollectionsExtensions
    {
        unsafe public static Span<T> AsSpan<T>(this NativeArray<T> array) where T : struct =>
            new(array.GetUnsafePtr(), array.Length);

        unsafe public static Span<T> AsSpan<T>(this NativeList<T> list) where T : unmanaged =>
            new(list.GetUnsafePtr(), list.Length);

        unsafe public static ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeArray<T> array) where T : struct =>
            new(array.GetUnsafeReadOnlyPtr(), array.Length);

        unsafe public static ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeArray<T>.ReadOnly array) where T : struct =>
            new(array.GetUnsafeReadOnlyPtr(), array.Length);

        unsafe public static ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeList<T> list) where T : unmanaged =>
            new(list.GetUnsafeReadOnlyPtr(), list.Length);
    }
}