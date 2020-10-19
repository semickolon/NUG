using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestExpectedResult
  {
    [TestCase(ExpectedResult = 23)]
    public int IsTrue()
    {
      Assert.That(true);
      return 23;
    }
  }
}