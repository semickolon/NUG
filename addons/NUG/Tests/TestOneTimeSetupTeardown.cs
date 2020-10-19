using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestOneTimeSetupTeardown
  {
    private static int _a;
    
    [OneTimeSetUp]
    public void SetupOnce()
    {
      _a++;
    }

    [OneTimeTearDown]
    public void TeardownOnce()
    {
      _a--;
      Assert.AreEqual(0, _a);
    }

    [Test]
    public void CheckA()
    {
      Assert.AreEqual(1, _a);
    }

    [Test]
    public void CheckB()
    {
      Assert.AreEqual(2 - 1, _a);
    }

    [Test]
    public void CheckC()
    {
      Assert.AreEqual(42 - 43 + 2, _a);
    }
  }
}