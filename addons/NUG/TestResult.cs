using System;
using System.Reflection;

namespace NUG
{
  public readonly struct TestResult
  {
    public readonly MethodInfo TestMethod;
    public readonly Exception? Exception;

    public Type ClassType => TestMethod.DeclaringType!;
    public bool Passed => Exception == null;

    public TestResult(MethodInfo testMethod, Exception? exception = null)
    {
      TestMethod = testMethod;
      Exception = exception;
    }
  }
}