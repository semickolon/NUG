using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestAbstractFixture
  {
    protected virtual int CreateNaturalNumber() => 4;

    [Test]
    public void IsNaturalNumber()
    {
      var n = CreateNaturalNumber();
      Assert.Greater(n, 0);
    }
  }
  
  public class TestAbstractFixture1 : TestAbstractFixture
  {
    protected override int CreateNaturalNumber() => 99;
  }
  
  public class TestAbstractFixture2 : TestAbstractFixture
  {
    protected override int CreateNaturalNumber() => 189322;
  }
}