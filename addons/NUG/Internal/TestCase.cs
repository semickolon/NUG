using System.Reflection;

namespace NUG.Internal
{
  public readonly struct TestCase
  {
    public readonly MethodInfo Method;
    public readonly object[] Parameters;
    public readonly object? ExpectedResult;
    public readonly int Order;

    public TestCase(MethodInfo method, object[] parameters, object? expectedResult, int order)
    {
      Method = method;
      Parameters = parameters;
      ExpectedResult = expectedResult;
      Order = order;
    }
  }
}