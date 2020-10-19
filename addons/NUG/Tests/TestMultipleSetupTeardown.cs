using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestMultipleSetupTeardown
  {
    private int _a;
    private int _b;
    
    [SetUp]
    public void SetUpA()
    {
      _a = 2;
    }
    
    [SetUp]
    public void SetUpB()
    {
      _b = 3;
    }

    [TestCase(5)]
    public void IsSumCorrect(int sum)
    {
      Assert.AreEqual(sum, _a + _b);
      _a = 75;
      _b = 25;
    }

    [TearDown]
    public void TearDownA()
    {
      Assert.AreEqual(75, _a);
    }

    [TearDown]
    public void TearDownB()
    {
      Assert.AreEqual(25, _b);
    }
  }
}