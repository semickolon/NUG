using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture(3)]
  [TestFixture(20)]
  [TestFixture(-1337)]
  public class TestOneTimeSetupTeardownMultipleFixtures
  {
    private static int _a;
    private static int _b;
    
    private int _n = 1;
    private readonly int _multiple;

    public TestOneTimeSetupTeardownMultipleFixtures(int multiple)
    {
      _multiple = multiple;
    }

    [OneTimeSetUp]
    public void SetupOnce()
    {
      _a += 23;
      _b++;
    }

    [OneTimeTearDown]
    public void TeardownOnce()
    {
      Assert.AreEqual(23 * _b, _a);
    }

    [SetUp]
    public void Setup()
    {
      _n--;
      _n += _multiple;
      _a--;
    }

    [TearDown]
    public void Teardown()
    {
      _a++;
    }
    
    [Test]
    public void IsMultipleA() 
    {
      Assert.That(_n % _multiple == 0);
    }
    
    [Test]
    public void IsMultipleB() 
    {
      Assert.That(_n % _multiple == 0);
    }
  }
}