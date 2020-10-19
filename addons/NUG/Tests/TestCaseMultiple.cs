using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestCaseMultiple
  {
    [TestCase(0, 1, 0)]
    [TestCase(-2, 2, -4)]
    [TestCase(4, 2, 8)]
    public void IsProductCorrect(int a, int b, int c)
    {
      Assert.AreEqual(a * b, c);
    }
  }
}