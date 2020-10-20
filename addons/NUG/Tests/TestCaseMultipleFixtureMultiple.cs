using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture(2)]
  [TestFixture(4)]
  public class TestCaseMultipleFixtureMultiple
  {
    private readonly int _n;
    private static int _t = 0;
    
    public TestCaseMultipleFixtureMultiple(int n)
    {
      _n = n;
    }
      
    [Order(1)]
    [TestCase(50)]
    [TestCase(4)]
    public void IsProductNaturalNumber(int a)
    {
      _t++;
      Assert.Greater(_n * a, 0);
    }

    [Order(2)]
    [Test]
    public void RanNTestCases()
    {
      Assert.AreEqual(_t, _n);
    }
  }
}