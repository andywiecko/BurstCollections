using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class NativeStackEditorTests
    {
        private NativeStack<int> stack;

        [TearDown]
        public void TearDown()
        {
            if (stack.IsCreated)
            {
                stack.Dispose();
            }
        }

        [Test]
        public void IsEmptyTest()
        {
            stack = new NativeStack<int>(capacity: 10, Allocator.Persistent);
            stack.Push(default);
            Assert.That(stack.IsEmpty, Is.False);
        }

        [Test]
        public void ThrowsEmptyTest()
        {
            stack = new NativeStack<int>(capacity: 10, Allocator.Persistent);
            stack.Push(default);
            stack.Pop();
            Assert.Throws<InvalidOperationException>(() => stack.Pop());
        }

        [Test]
        public void ClearTest()
        {
            stack = new NativeStack<int>(capacity: 10, Allocator.Persistent);
            stack.Push(default);
            stack.Push(default);
            stack.Push(default);
            stack.Clear();
            Assert.That(stack.IsEmpty, Is.True);
        }

        [Test]
        public void TryPopTest()
        {
            stack = new NativeStack<int>(capacity: 10, Allocator.Persistent);
            stack.Push(default);
            var firstPop = stack.TryPop(out var _);
            var secondPop = stack.TryPop(out var _);
            Assert.That(firstPop, Is.True);
            Assert.That(secondPop, Is.False);
        }

        [Test]
        public void StackTest()
        {
            stack = new NativeStack<int>(capacity: 10, Allocator.Persistent);

            stack.Push(0);
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);
            var popedItems = new List<int>();
            while (stack.TryPop(out var item))
            {
                popedItems.Add(item);
            }

            Assert.That(popedItems, Is.EqualTo(new[] { 3, 2, 1, 0 }));
        }

        [Test]
        public void LengthTest()
        {
            stack = new NativeStack<int>(capacity: 10, Allocator.Persistent);

            stack.Push(0);
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);

            Assert.That(stack.Length, Is.EqualTo(4));
        }
    }
}