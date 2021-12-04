using NUnit.Framework;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class IdEditorTests
    {
        private readonly struct Fake { }

        [Test]
        public void PostIncrementOperatorTest()
        {
            var id = (Id<Fake>)0;
            Assert.That(id, Is.EqualTo((Id<Fake>)0));
            Assert.That(id++, Is.EqualTo((Id<Fake>)0));
            Assert.That(id, Is.EqualTo((Id<Fake>)1));
        }

        [Test]
        public void PreIncrementOperatorTest()
        {
            var id = (Id<Fake>)0;
            Assert.That(id, Is.EqualTo((Id<Fake>)0));
            Assert.That(++id, Is.EqualTo((Id<Fake>)1));
            Assert.That(id, Is.EqualTo((Id<Fake>)1));
        }

        [Test]
        public void BooleanOperatorsTest()
        {
            Assert.That((Id<Fake>)0 == (Id<Fake>)0, Is.True);
            Assert.That((Id<Fake>)0 == (Id<Fake>)1, Is.False);

            Assert.That((Id<Fake>)0 != (Id<Fake>)0, Is.False);
            Assert.That((Id<Fake>)0 != (Id<Fake>)1, Is.True);

            Assert.That((Id<Fake>)0 < (Id<Fake>)0, Is.False);
            Assert.That((Id<Fake>)0 < (Id<Fake>)1, Is.True);

            Assert.That((Id<Fake>)0 > (Id<Fake>)0, Is.False);
            Assert.That((Id<Fake>)1 > (Id<Fake>)0, Is.True);

            Assert.That((Id<Fake>)0 <= (Id<Fake>)0, Is.True);
            Assert.That((Id<Fake>)1 <= (Id<Fake>)0, Is.False);

            Assert.That((Id<Fake>)0 >= (Id<Fake>)0, Is.True);
            Assert.That((Id<Fake>)0 >= (Id<Fake>)1, Is.False);
        }

        [Test]
        public void InvalidTest()
        {
            Assert.That(Id<Fake>.Invalid, Is.EqualTo((Id<Fake>)(-1)));
        }

        [Test]
        public void ZeroTest()
        {
            Assert.That(Id<Fake>.Zero, Is.EqualTo((Id<Fake>)0));
        }

        [Test]
        public void ArtimeticOperatorsTest()
        {
            Assert.That((Id<Fake>)2 + 1, Is.EqualTo((Id<Fake>)3));
            Assert.That((Id<Fake>)2 - 1, Is.EqualTo((Id<Fake>)1));

            Assert.That((Id<Fake>)2 + (Id<Fake>)1, Is.EqualTo((Id<Fake>)3));
            Assert.That((Id<Fake>)2 - (Id<Fake>)1, Is.EqualTo((Id<Fake>)1));
        }
    }
}