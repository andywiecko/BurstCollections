using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class NativeStackedListsEditorTests
    {
        [Test]
        public void InitTest()
        {
            using var stackedLists = new NativeStackedLists<int>(64, 4, Allocator.Persistent);
            Assert.That(stackedLists.Length, Is.Zero);
        }

        [Test]
        public void ThrowsOnAddWhenLengthZero()
        {
            using var stackedLists = new NativeStackedLists<int>(64, 4, Allocator.Persistent);
            Assert.Throws<IndexOutOfRangeException>(() => stackedLists.Add(default));
        }

        [Test]
        public void PushTest([Values(1, 3, 5)] int count)
        {
            using var stackedLists = new NativeStackedLists<int>(64, 4, Allocator.Persistent);

            for (int i = 0; i < count; i++)
            {
                stackedLists.Push();
            }

            Assert.That(stackedLists.Length, Is.EqualTo(count));
        }

        [Test]
        public void AddTest()
        {
            using var stackedLists = new NativeStackedLists<int>(64, 4, Allocator.Persistent);

            stackedLists.Push();
            stackedLists.Add(1);
            stackedLists.Add(6);
            stackedLists.Add(8);

            Assert.That(stackedLists[0][0], Is.EqualTo(1));
            Assert.That(stackedLists[0][1], Is.EqualTo(6));
            Assert.That(stackedLists[0][2], Is.EqualTo(8));
        }

        [Test]
        public void ClearTest()
        {
            using var stackedLists = new NativeStackedLists<int>(64, 4, Allocator.Persistent);

            stackedLists.Push();
            stackedLists.Add(1);
            stackedLists.Add(6);
            stackedLists.Add(8);

            var wasNotEmpty = stackedLists.Length != 0;
            stackedLists.Clear();

            Assert.That(wasNotEmpty, Is.True);
            Assert.That(stackedLists, Has.Length.Zero);
        }

        [Test]
        public void EnumerableTest()
        {
            using var stackedLists = new NativeStackedLists<int>(64, 4, Allocator.Persistent);

            stackedLists.Push();
            stackedLists.Add(1);
            stackedLists.Add(2);
            stackedLists.Add(4);

            stackedLists.Push();
            stackedLists.Add(1);
            stackedLists.Add(3);
            stackedLists.Add(9);
            stackedLists.Add(27);

            int[][] expected =
            {
                new int[] { 1, 2, 4 },
                new int[] { 1, 3, 9, 27 }
            };
            Assert.That(stackedLists, Is.EqualTo(expected));
        }
    }
}