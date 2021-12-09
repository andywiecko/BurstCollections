using NUnit.Framework;
using System.Collections.Generic;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class IdEnumeratorsEditorTests
    {
        [Test]
        public void IdEnumeratorTest()
        {
            var iter = new IdEnumerator<Id<int>>(start: 2, length: 4);

            var result = new List<Id<int>>();
            foreach (var i in iter)
            {
                result.Add(i);
            }

            var expectedResult = new[] 
            { 
                (Id<int>)2, (Id<int>)3, (Id<int>)4, (Id<int>)5 
            };
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void IdValueEnumeratorTest()
        {
            var a = new[] { 1, 24, 3 };
            var iter = new IdValueEnumerator<Id<int>, int>(a);

            var result = new List<(Id<int>, int)>();
            foreach (var tuple in new IdValueEnumerator<Id<int>, int>(a))
            {
                result.Add(tuple);
            }

            var expectedResult = new[]
            {
                ((Id<int>)0, 1), ((Id<int>)1, 24),((Id<int>)2, 3),
            };
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}