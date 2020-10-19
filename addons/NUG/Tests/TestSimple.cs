using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestSimple
  {
    [Test]
    public void IsTrue()
    {
      Assert.IsTrue(1 + 1 == 2);
    }
    
    [Test]
    public void IsFalse()
    {
      Assert.IsFalse(1 == 2);
    }
  }
}