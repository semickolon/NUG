using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestCaseMultiple
  {
    private static int _t = 0;
    
    [Order(1)]
    [TestCase(0, 1, 0)]
    [TestCase(-2, 2, -4)]
    [TestCase(4, 2, 8)]
    public void IsProductCorrect(int a, int b, int c)
    {
      _t++;
      Assert.AreEqual(a * b, c);
    }

    [Order(2)]
    [Test]
    public void RanThreeTestCases()
    {
      Assert.AreEqual(3, _t);
    }
  }
}