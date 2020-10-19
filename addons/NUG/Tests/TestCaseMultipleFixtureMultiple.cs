using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture(1)]
  [TestFixture(2)]
  public class TestCaseMultipleFixtureMultiple
  {
    private readonly int _n;
    
    public TestCaseMultipleFixtureMultiple(int n)
    {
      _n = n;
    }
      
    [TestCase(50)]
    [TestCase(4)]
    public void IsProductNaturalNumber(int a)
    {
      Assert.Greater(_n * a, 0);
    }
  }
}