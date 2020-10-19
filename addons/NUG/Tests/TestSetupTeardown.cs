using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestSetupTeardown
  {
    private int _n;
    
    [SetUp]
    public void Setup()
    {
      _n = 1;
    }

    [Test]
    public void IsNumberOne()
    {
      Assert.AreEqual(1, _n);
      _n = 2;
    }

    [TearDown]
    public void Teardown()
    {
      Assert.AreEqual(2, _n);
    }
  }
}