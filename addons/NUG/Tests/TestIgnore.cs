using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestIgnore
  {
    private static int _x = 42;
    
    [Test]
    [Order(1)]
    [Ignore("Just please don't run")]
    public void DontRun()
    {
      _x = 182398;
      Assert.That(false);
    }

    [Test]
    [Order(2)]
    public void IgnoreDidNotRun()
    {
      Assert.AreEqual(42, _x);
    }
    
    [Test]
    [Order(3)]
    [Ignore("Do run", Until = "2000-01-01")]
    public void DoRunUntil()
    {
      _x = 100;
      Assert.That(true);
    }

    [Test]
    [Order(4)]
    public void IgnoreUntilDidRun()
    {
      Assert.AreEqual(100, _x);
    }
    
    [Test]
    [Order(5)]
    [Ignore("Do not run ever (unless it's 2100)", Until = "2100-01-01")]
    public void DoNotRunUntil()
    {
      _x = 69420;
      Assert.That(false);
    }

    [Test]
    [Order(6)]
    public void IgnoreUntilDidNotRun()
    {
      Assert.AreEqual(100, _x);
    }
  }
}