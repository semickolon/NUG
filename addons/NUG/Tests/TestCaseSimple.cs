using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestCaseSimple
  {
    [TestCase]
    public void IsTrue()
    {
      Assert.That(40 > 20);
    }

    [TestCase(Ignore = "Ignore me")]
    public void DontRun()
    {
      Assert.That(1 == 2);
    }

    [TestCase("Foobar", "Qux")]
    public void DoesNotContainPart(string whole, string part)
    {
      Assert.IsFalse(whole.Contains(part));
    }
  }
}