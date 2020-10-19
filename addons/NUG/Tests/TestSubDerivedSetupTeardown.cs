using System.Collections.Generic;
using NUnit.Framework;

namespace NUG.Tests
{
  [TestFixture] // Order intentionally messed up to make sure pass isn't a fluke
  public class TestSubDerivedSetupTeardown : TestDerivedSetupTeardown
  {
    [Test]
    public void IsSetupOrderCorrect()
    {
      Assert.AreEqual(new List<int> {0, 1, 2, 3}, Nums);
    }

    [TearDown]
    public void SubDerivedTeardown()
    {
      Nums.Add(4);
      Assert.AreEqual(new List<int> {0, 1, 2, 3, 4}, Nums);
    }
    
    [SetUp]
    public void SubDerivedSetup()
    {
      Nums.Add(3);
    }
  }
  
  public class TestBaseSetupTeardown
  {
    protected static readonly List<int> Nums = new List<int> { 0 };

    [SetUp]
    public void BaseSetup()
    {
      Nums.Add(1);
    }
      
    [TearDown]
    public void BaseTeardown()
    {
      Nums.Add(6);
      Assert.AreEqual(new List<int> { 0, 1, 2, 3, 4, 5, 6 }, Nums);
    }
  }
  
  public class TestDerivedSetupTeardown : TestBaseSetupTeardown
  {
    [TearDown]
    public void DerivedTeardown()
    {
      Nums.Add(5);
      Assert.AreEqual(new List<int> { 0, 1, 2, 3, 4, 5 }, Nums);
    }
    
    [SetUp]
    public void DerivedSetup()
    {
      Nums.Add(2);
    }
  }
}