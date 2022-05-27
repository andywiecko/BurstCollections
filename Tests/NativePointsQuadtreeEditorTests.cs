using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class NativePointsQuadtreeEditorTests
    {
        [Test]
        public void Test()
        {
            var tree = new NativePointQuadtree(new(0, 10), 1024, 4, Allocator.Persistent);

            var random = new Unity.Mathematics.Random(seed: 89834u);
            var points = Enumerable.Range(0, 10).Select(i => 10 * random.NextFloat2()).ToArray();

            tree.Build(points);

            var a = 4;

            tree.Dispose();
        }
    }
}