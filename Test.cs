using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class Test
  {
    [Test]
    public void IsTrue()
    {
      Assert.IsTrue(1 == 1);
    }
    
    [Test]
    public void IsFalse()
    {
      Assert.IsFalse(1 == 2);
    }
  }

  [TestFixture(5, 7, 12)]
  public class TestFixtureArgs
  {
    private readonly int _a;
    private readonly int _b;
    private readonly int _sum;

    public TestFixtureArgs(int a, int b, int sum)
    {
      _a = a;
      _b = b;
      _sum = sum;
    }
    
    [Test]
    public void AddsArgs()
    {
      Assert.That(_a + _b == _sum);
    }
  }
}