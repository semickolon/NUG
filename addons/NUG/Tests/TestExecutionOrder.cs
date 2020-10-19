using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestExecutionOrder
  {
    private static int _n;

    [Test, Order(30)]
    public void IsThird()
    {
      _n++;
      Assert.AreEqual(3, _n);
    }

    [Test]
    [Order(0)]
    public void IsFirst()
    {
      _n++;
      Assert.AreEqual(1, _n);
    }

    [Test]
    public void IsLast()
    {
      _n++;
      Assert.AreEqual(4, _n);
    }

    [Test]
    [Order(10)]
    public void IsSecond()
    {
      _n++;
      Assert.AreEqual(2, _n);
    }
  }
}