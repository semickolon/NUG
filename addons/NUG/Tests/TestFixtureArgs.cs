using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture("Hello World", "ello")]
  public class TestFixtureArgs
  {
    private readonly string _whole;
    private readonly string _part;
    
    public TestFixtureArgs(string whole, string part)
    {
      _whole = whole;
      _part = part;
    }

    [Test]
    public void ContainsPart()
    {
      Assert.That(_whole.Contains(_part));
    }
  }
}