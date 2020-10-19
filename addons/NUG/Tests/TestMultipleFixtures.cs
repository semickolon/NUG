using NUnit.Framework;

namespace NUG.Tests
{
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
      Assert.AreEqual(_sum, _a + _b);
    }
  }
}