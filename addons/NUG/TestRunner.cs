using Godot;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUG.Internal;
using TestContext = NUG.Internal.TestContext;

namespace NUG
{
  public class TestRunner
  {
    private readonly SceneTree _sceneTree;
    private readonly Dictionary<TestContext, List<TestCase>> _testCases = new Dictionary<TestContext, List<TestCase>>();
    
    private readonly MethodRegistry _setupMethods = new MethodRegistry();
    private readonly MethodRegistry _teardownMethods = new MethodRegistry(true);
    private readonly MethodRegistry _oneTimeSetupMethods = new MethodRegistry();
    private readonly MethodRegistry _oneTimeTeardownMethods = new MethodRegistry(true);
    
    public TestRunner(SceneTree sceneTree)
    {
      _sceneTree = sceneTree;
      ScanAssemblies();
    }

    public async Task Run(Action<TestResult> callback)
    {
      foreach (var context in _testCases.Keys)
      {
        var testCases = _testCases[context]!;

        _oneTimeSetupMethods.InvokeAll(context);
        
        foreach (var testCase in testCases)
        {
          var testResult = await RunTestCase(context, testCase);
          callback(testResult);
        }

        _oneTimeTeardownMethods.InvokeAll(context);
      }
    }

    private async Task<TestResult> RunTestCase(TestContext context, TestCase testCase)
    {
      var testObject = context.CreateTestObject();
      var method = testCase.Method;
      var testResult = new TestResult(method);
      
      try
      {
        if (testObject is Node node)
        {
          _sceneTree.Root.AddChild(node);
        }

        _setupMethods.InvokeAll(context, testObject);

        object obj = method.Invoke(testObject, testCase.Parameters);

        if (obj is IEnumerator coroutine)
        {
          while (coroutine.MoveNext())
          {
            await Task.Delay(10);
          }
        }

        testCase.ExpectedResult?.With(x => Assert.AreEqual(x, obj));
      }
      catch (Exception e)
      {
        testResult = new TestResult(method, e.InnerException ?? e);
      }
      finally
      {
        _teardownMethods.InvokeAll(context, testObject);

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
          
          var constructor = FindConstructor(concreteType, testFixtureAttr.Arguments);
          object CreateTestObject() => constructor!.Invoke(testFixtureAttr.Arguments);

          var testCases = new List<TestCase>();
          var context = new TestContext(concreteType, CreateTestObject);
          
          foreach (var method in concreteType.GetMethods())
          {
            var testCase = ScanMethod(context, method);
            testCase?.With(x => testCases.Add(x));
          }

          if (testCases.Count == 0)
            continue;
          
          testCases.Sort((x, y) => x.Order - y.Order);
          _testCases[context] = testCases;
        }
      }
    }

    private TestCase? ScanMethod(TestContext context, MethodInfo method)
    {
      if (HasCustomAttribute<SetUpAttribute>(method))
      {
        _setupMethods.AddMethod(context, method);
      }
      else if (HasCustomAttribute<TearDownAttribute>(method))
      {
        _teardownMethods.AddMethod(context, method);
      }
      else if (HasCustomAttribute<OneTimeSetUpAttribute>(method))
      {
        _oneTimeSetupMethods.AddMethod(context, method);
      }
      else if (HasCustomAttribute<OneTimeTearDownAttribute>(method))
      {
        _oneTimeTeardownMethods.AddMethod(context, method);
      }
      else
      {
        var testCaseAttrs = GetCustomAttributes<TestCaseAttribute>(method).ToList();
        
        if (testCaseAttrs.Count == 0)
        {
          var testAttr = GetCustomAttribute<TestAttribute>(method);
          if (testAttr != null)
          {
            var testCaseAttr = ToTestCaseAttribute(testAttr);
            testCaseAttrs.Add(testCaseAttr);
          }
        }
        
        foreach (var testCaseAttr in testCaseAttrs)
        {
          if (testCaseAttr == null || testCaseAttr.Ignore != null || testCaseAttr.IgnoreReason != null)
            return null;

          var orderAttr = GetCustomAttribute<OrderAttribute>(method);
          var order = orderAttr?.Order ?? int.MaxValue;
          
          var testCase = new TestCase(method, testCaseAttr.Arguments, testCaseAttr.ExpectedResult, order);
          return testCase;
        }
      }

      return null;
    }

    private static ConstructorInfo? FindConstructor(Type type, object[] args)
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

    private static bool HasCustomAttribute<T>(MemberInfo type) where T : Attribute
    {
      return Attribute.IsDefined(type, typeof(T));
    }
    
    private static T? GetCustomAttribute<T>(MemberInfo type) where T : Attribute
    {
      return Attribute.GetCustomAttribute(type, typeof(T)) as T;
    }
    
    private static T[] GetCustomAttributes<T>(MemberInfo type) where T : Attribute
    {
      return (T[]) Attribute.GetCustomAttributes(type, typeof(T));
    }
    
    private static TestCaseAttribute ToTestCaseAttribute(TestAttribute testAttr)
    {
      var testCaseAttr = new TestCaseAttribute();
      testAttr.Author?.With(x => testCaseAttr.Author = x);
      testAttr.Description?.With(x => testCaseAttr.Description = x);
      testAttr.ExpectedResult?.With(x => testCaseAttr.ExpectedResult = x);
      testAttr.TestOf?.With(x => testCaseAttr.TestOf = x);
      return testCaseAttr;
    }
  }
}