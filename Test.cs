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

  [TestFixture("Hello World", "ello")]
  public class TestFixtureArgs
  {
    private readonly string _whole;
    private readonly string _part;
    
    public TestFixtureArgs(string whole, string part)
    {
      _whole = whole;
      _part = part;
    }

    [Test]
    public void ContainsPart()
    {
      Assert.That(_whole.Contains(_part));
    }
  }

  [TestFixture(5, 7, 12)]
  [TestFixture(60, 9, 69)]
  [TestFixture(1, -1, 0)]
  public class TestMultipleFixtures
  {
    private readonly int _a;
    private readonly int _b;
    private readonly int _sum;

    public TestMultipleFixtures(int a, int b, int sum)
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