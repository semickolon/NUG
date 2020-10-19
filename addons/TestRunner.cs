using Godot;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NUG
{
  public class TestRunner
  {
    private readonly SceneTree _sceneTree;
    private readonly Dictionary<Type, List<TestCase>> _testCases = new Dictionary<Type, List<TestCase>>();
    private readonly Dictionary<Type, MethodInfo> _setupMethods = new Dictionary<Type, MethodInfo>();
    private readonly Dictionary<Type, MethodInfo> _teardownMethods = new Dictionary<Type, MethodInfo>();

    public TestRunner(SceneTree sceneTree)
    {
      _sceneTree = sceneTree;
      ScanAssemblies();
    }

    public async Task Run(Action<TestResult> callback)
    {
      foreach (var type in _testCases.Keys)
      {
        var testCases = _testCases[type]!;
        foreach (var testCase in testCases)
        {
          var testResult = await RunTestCase(testCase);
          callback(testResult);
        }
      }
    }

    private async Task<TestResult> RunTestCase(TestCase testCase)
    {
      var method = testCase.Method;
      var testObject = testCase.TestObject;
      
      var testResult = new TestResult(method);
      
      try
      {
        if (testObject is Node node)
        {
          _sceneTree.Root.AddChild(node);
        }

        _setupMethods.TryGetValue(method.DeclaringType, out var setupMethod);
        setupMethod?.Invoke(testObject, new object[] { });

        object obj = method.Invoke(testObject, testCase.Parameters);

        if (obj is IEnumerator coroutine)
        {
          while (coroutine.MoveNext())
          {
            await Task.Delay(10);
          }
        }
      }
      catch (Exception e)
      {
        testResult = new TestResult(method, e.InnerException ?? e);
      }
      finally
      {
        _teardownMethods.TryGetValue(method.DeclaringType, out var teardownMethod);
        teardownMethod?.Invoke(testObject, new object[] { });

        (testObject as Node)?.QueueFree();
      }

      return testResult;
    }

    private void ScanAssemblies()
    {
      var assemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (var assembly in assemblies)
      {
        ScanTypes(assembly.GetTypes());
      }
    }

    private void ScanTypes(Type[] types)
    {
      foreach (var type in types)
      {
        var testFixtureAttrs = GetCustomAttributes<TestFixtureAttribute>(type);
        
        foreach (var testFixtureAttr in testFixtureAttrs)
        {
          var concreteType = type;
          if (type.ContainsGenericParameters)
          {
            concreteType = type.MakeGenericType(testFixtureAttr.TypeArgs);
          }
          
          var constructor = TryGetConstructor(concreteType, testFixtureAttr.Arguments);
          if (constructor == null)
            continue; // TODO really?
          
          var methods = concreteType.GetMethods();
          var testCases = new List<TestCase>();
          
          object CreateTestObject() => constructor.Invoke(testFixtureAttr.Arguments);
          
          foreach (var method in methods)
          {
            var testCase = ScanMethod(concreteType, method, CreateTestObject);
            testCase?.With(x => testCases.Add(x));
          }

          if (testCases.Count > 0)
          {
            testCases.Sort((a, b) => a.Order - b.Order);
            _testCases[concreteType] = testCases;
          }
        }
      }
    }

    private TestCase? ScanMethod(Type type, MethodInfo method, Func<object> createTestObject)
    {
      var testCaseAttrs = GetCustomAttributes<TestCaseAttribute>(method).ToList();
      var orderAttr = GetCustomAttribute<OrderAttribute>(method);
      var setupAttr = GetCustomAttribute<SetUpAttribute>(method);
      var teardownAttr = GetCustomAttribute<TearDownAttribute>(method);

      if (setupAttr != null)
      {
        _setupMethods[type] = method;
      }
      else if (teardownAttr != null)
      {
        _teardownMethods[type] = method;
      }
      else
      {
        if (testCaseAttrs.Count == 0)
        {
          var testAttr = GetCustomAttribute<TestAttribute>(method);
          var testCaseAttr = testAttr?.ToTestCaseAttribute();
          testCaseAttr?.With(x => testCaseAttrs.Add(x));
        }
        
        foreach (var testCaseAttr in testCaseAttrs)
        {
          if (testCaseAttr == null || testCaseAttr.Ignore != null)
            return null;

          var order = orderAttr?.Order ?? Int32.MaxValue;
          var testPayload = new TestCase(method, createTestObject(), testCaseAttr.Arguments, order);
          return testPayload;
        }
      }

      return null;
    }

    private static ConstructorInfo? TryGetConstructor(Type type, object[] args)
    {
      var argTypes = args
        .Select(arg => arg.GetType())
        .ToArray();
      
      foreach (var constructor in type.GetConstructors())
      {
        var constructorArgTypes = constructor.GetParameters()
          .Select(p => p.ParameterType)
          .ToArray();

        if (argTypes.SequenceEqual(constructorArgTypes))
          return constructor;
      }

      return null;
    }
    
    private static T? GetCustomAttribute<T>(MemberInfo type) where T : Attribute
    {
      return Attribute.GetCustomAttribute(type, typeof(T)) as T;
    }
    
    private static T[] GetCustomAttributes<T>(MemberInfo type) where T : Attribute
    {
      return (T[]) Attribute.GetCustomAttributes(type, typeof(T));
    }
  }

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

  public readonly struct TestCase
  {
    public readonly MethodInfo Method;
    public readonly object TestObject;
    public readonly object[] Parameters;
    public readonly int Order;

    public TestCase(MethodInfo method, object testObject, object[] parameters, int order)
    {
      Method = method;
      TestObject = testObject;
      Parameters = parameters;
      Order = order;
    }
  }

  public static class NUnitExtensions
  {
    public static TestCaseAttribute ToTestCaseAttribute(this TestAttribute testAttr)
    {
      var testCaseAttr = new TestCaseAttribute();
      testAttr.Author?.With(x => testCaseAttr.Author = x);
      testAttr.Description?.With(x => testCaseAttr.Description = x);
      testAttr.ExpectedResult?.With(x => testCaseAttr.ExpectedResult = x);
      testAttr.TestOf?.With(x => testCaseAttr.TestOf = x);
      return testCaseAttr;
    }
  }

  public static class GeneralExtensions
  {
    public static void With<T>(this T t, Action<T> action)
    {
      action(t);
    }
  }
}