using System;

namespace NUG.Internal
{
  public readonly struct TestContext
  {
    public readonly Type Type;
    public readonly Func<object> CreateTestObject;

    public TestContext(Type type, Func<object> createTestObject)
    {
      Type = type;
      CreateTestObject = createTestObject;
    }
  }
}