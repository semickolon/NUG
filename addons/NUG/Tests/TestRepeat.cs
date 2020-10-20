using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestRepeat
  {
    private static int _t = 0;

    [Test, Repeat(5), Order(1)]
    public void RepeatWorking()
    {
      _t++;
      Assert.LessOrEqual(_t, 5);
    }

    [Test, Order(2)]
    public void RepeatWorked()
    {
      Assert.AreEqual(_t, 5);
    }
  }

  [TestFixture]
  public class TestRepeatMultipleCases
  {
    private static readonly List<int> Nums = new List<int>();

    [Repeat(2)]
    [Order(1)]
    [TestCase(10, -20, -10)]
    [TestCase(1, 2, 3)]
    [TestCase(10, 32, 42)]
    public void IsSumCorrect(int a, int b, int sum)
    {
      Nums.Add(a + b);
      Assert.AreEqual(sum, Nums.Last());
    }

    [Test, Order(2)]
    public void AreSumsCorrect()
    {
      Assert.AreEqual(new List<int> { -10, -10, 3, 3, 42, 42 }, Nums);
    }
  }
}