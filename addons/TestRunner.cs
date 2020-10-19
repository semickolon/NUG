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
    private readonly Dictionary<MethodInfo, List<TestPayload>> _testMethods = new Dictionary<MethodInfo, List<TestPayload>>();
    private readonly Dictionary<Type, MethodInfo> _setupMethods = new Dictionary<Type, MethodInfo>();
    private readonly Dictionary<Type, MethodInfo> _teardownMethods = new Dictionary<Type, MethodInfo>();

    public TestRunner(SceneTree sceneTree)
    {
      _sceneTree = sceneTree;
      ScanAssemblies();
    }

    public async Task Run(Action<TestResult> callback)
    {
      foreach (var method in _testMethods.Keys)
      {
        var testPayloads = _testMethods[method]!;
        foreach (var testPayload in testPayloads)
        {
          var testResult = await RunTest(method, testPayload);
          callback(testResult);
        }
      }
    }

    private async Task<TestResult> RunTest(MethodInfo method, TestPayload testPayload)
    {
      var testResult = new TestResult(method);
      var testObject = testPayload.TestObject;
      
      try
      {
        if (testObject is Node node)
        {
          _sceneTree.Root.AddChild(node);
        }

        _setupMethods.TryGetValue(method.DeclaringType, out var setupMethod);
        setupMethod?.Invoke(testObject, new object[] { });

        object obj = method.Invoke(testObject, testPayload.Parameters);

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

          object CreateTestObject() => constructor.Invoke(testFixtureAttr.Arguments);

          var methods = concreteType.GetMethods();
          foreach (var method in methods)
          {
            ScanMethod(concreteType, method, CreateTestObject);
          }
        }
      }
    }

    private void ScanMethod(Type type, MethodInfo method, Func<object> createTestObject)
    {
      var testCaseAttrs = GetCustomAttributes<TestCaseAttribute>(method).ToList();
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
          if (testCaseAttr != null)
          {
            testCaseAttrs.Add(testCaseAttr);
          }
        }
        
        foreach (var testCaseAttr in testCaseAttrs)
        {
          if (testCaseAttr == null || testCaseAttr.Ignore != null)
            return;
        
          if (!_testMethods.ContainsKey(method))
          {
            _testMethods[method] = new List<TestPayload>();
          }

          var testPayload = new TestPayload(createTestObject(), testCaseAttr.Arguments);
          _testMethods[method].Add(testPayload);
        }
      }
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

  public readonly struct TestPayload
  {
    public readonly object TestObject;
    public readonly object[] Parameters;

    public TestPayload(object testObject, object[] parameters)
    {
      TestObject = testObject;
      Parameters = parameters;
    }
  }

  public static class NUnitExtensions
  {
    public static TestCaseAttribute ToTestCaseAttribute(this TestAttribute testAttr)
    {
      var testCaseAttr = new TestCaseAttribute();

      if (testAttr.Author != null)
        testCaseAttr.Author = testAttr.Author;

      if (testAttr.Description != null)
        testCaseAttr.Description = testAttr.Description;

      if (testAttr.ExpectedResult != null)
        testCaseAttr.ExpectedResult = testAttr.ExpectedResult;

      if (testAttr.TestOf != null)
        testCaseAttr.TestOf = testAttr.TestOf;
        
      return testCaseAttr;
    }
  }
}