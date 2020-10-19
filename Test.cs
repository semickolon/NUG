using System.Collections.Generic;
using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture]
  public class TestSimple
  {
    [Test]
    public void IsTrue()
    {
      Assert.IsTrue(1 + 1 == 2);
    }
    
    [Test]
    public void IsFalse()
    {
      Assert.IsFalse(1 == 2);
    }
  }

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

  [TestFixture(TypeArgs = new [] { typeof(Godot.Object), typeof(Godot.Reference) })]
  public class TestGenericFixture<TBase, TSub>
  {
    [Test]
    public void HasCorrectBaseType()
    {
      Assert.AreEqual(typeof(TBase), typeof(TSub).BaseType);
    }
  }

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

  [TestFixture(1)]
  [TestFixture(2)]
  public class TestCaseMultipleFixtureMultiple
  {
    private readonly int _n;
    
    public TestCaseMultipleFixtureMultiple(int n)
    {
      _n = n;
    }
      
    [TestCase(50)]
    [TestCase(4)]
    public void IsProductNaturalNumber(int a)
    {
      Assert.Greater(_n * a, 0);
    }
  }

  [TestFixture]
  public class TestSetupTeardown
  {
    private int _n = 0;
    
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

  [TestFixture]
  public class TestExecutionOrder
  {
    private static int _n = 0;

    [Test, Order(30)]
    public void IsThird()
    {
      _n++;
      Assert.AreEqual(3, _n);
    }

    [Test]
    [Order(0)]
    public void IsFirst()
    {
      _n++;
      Assert.AreEqual(1, _n);
    }

    [Test]
    public void IsLast()
    {
      _n++;
      Assert.AreEqual(4, _n);
    }

    [Test]
    [Order(10)]
    public void IsSecond()
    {
      _n++;
      Assert.AreEqual(2, _n);
    }
  }

  [TestFixture]
  public class TestExpectedResult
  {
    [TestCase(ExpectedResult = 23)]
    public int IsTrue()
    {
      Assert.That(true);
      return 23;
    }
  }

  [TestFixture]
  public class TestMultipleSetupTeardown
  {
    private int _a = 0;
    private int _b = 0;
    
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
  
  [TestFixture] // Order intentionally messed up to make sure pass isn't a fluke
  public class TestSubDerivedSetupTeardown : TestDerivedSetupTeardown
  {
    [Test]
    public void IsSetupOrderCorrect()
    {
      Assert.AreEqual(new List<int> {0, 1, 2, 3}, nums);
    }

    [TearDown]
    public void SubDerivedTeardown()
    {
      nums.Add(4);
      Assert.AreEqual(new List<int> {0, 1, 2, 3, 4}, nums);
    }
    
    [SetUp]
    public void SubDerivedSetup()
    {
      nums.Add(3);
    }
  }

  public class TestBaseSetupTeardown
  {
    protected static List<int> nums = new List<int> { 0 };

    [SetUp]
    public void BaseSetup()
    {
      nums.Add(1);
    }
      
    [TearDown]
    public void BaseTeardown()
    {
      nums.Add(6);
      Assert.AreEqual(new List<int> { 0, 1, 2, 3, 4, 5, 6 }, nums);
    }
  }

  public class TestDerivedSetupTeardown : TestBaseSetupTeardown
  {
    [TearDown]
    public void DerivedTeardown()
    {
      nums.Add(5);
      Assert.AreEqual(new List<int> { 0, 1, 2, 3, 4, 5 }, nums);
    }
    
    [SetUp]
    public void DerivedSetup()
    {
      nums.Add(2);
    }
  }
}