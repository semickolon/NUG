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
          
          var constructor = TryGetConstructor(concreteType, testFixtureAttr.Arguments);
          if (constructor == null)
            continue; // TODO really?

          var testCases = new List<TestCase>();
          object CreateTestObject() => constructor.Invoke(testFixtureAttr.Arguments);
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

  public static class GeneralExtensions
  {
    public static void With<T>(this T t, Action<T> action)
    {
      action(t);
    }
  }

  public class MethodRegistry
  {
    private readonly Dictionary<TestContext, List<MethodInfo>> _store = new Dictionary<TestContext, List<MethodInfo>>();
    private readonly HashSet<TestContext> _dirtySet = new HashSet<TestContext>();
    private readonly bool _reverseCallOrder;

    public MethodRegistry(bool reverseCallOrder = false)
    {
      _reverseCallOrder = reverseCallOrder;
    }
    
    public void InvokeAll(TestContext context, object? testObject = null)
    {
      GetMethods(context)?.ForEach(m =>
      {
        m.Invoke(testObject ?? context.CreateTestObject(), new object[] {});
      });
    }

    public void AddMethod(TestContext context, MethodInfo method)
    {
      if (!_store.ContainsKey(context))
      {
        _store[context] = new List<MethodInfo>();
      }
      
      _store[context].Add(method);
      SetDirty(context);
    }

    private List<MethodInfo>? GetMethods(TestContext context)
    {
      if (!_store.ContainsKey(context))
        return null;

      if (IsDirty(context))
      {
        SortFor(context);
      }

      return _store[context];
    }
    
    private bool IsDirty(TestContext context)
    {
      return _dirtySet.Contains(context);
    }

    private void SetDirty(TestContext context)
    {
      _dirtySet.Add(context);
    }

    private void SortFor(TestContext context)
    {
      _store[context].Sort(CompareByHierarchy);
      _dirtySet.Remove(context);
    }
    
    private int CompareByHierarchy(MethodInfo x, MethodInfo y)
    {
      var xt = x.DeclaringType!;
      var yt = y.DeclaringType!;

      if (xt == yt)
        return 0;

      var c = xt.IsSubclassOf(yt) ? 1 : -1;
      c *= _reverseCallOrder ? -1 : 1;
      return c;
    }
  }
}