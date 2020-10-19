using Godot;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework.Internal;

namespace NUG
{
  public class TestRunner
  {
    private readonly SceneTree _sceneTree;
    private readonly Dictionary<MethodInfo, object> _testMethods = new Dictionary<MethodInfo, object>();
    private readonly Dictionary<Type, MethodInfo> _setupMethods = new Dictionary<Type, MethodInfo>();
    private readonly Dictionary<Type, MethodInfo> _teardownMethods = new Dictionary<Type, MethodInfo>();

    public TestRunner(SceneTree sceneTree)
    {
      _sceneTree = sceneTree;
      ScanAssemblies();
    }

    public async Task Run(Action<TestResult> action)
    {
      foreach (var method in _testMethods.Keys)
      {
        var testObject = _testMethods[method];
        var testResult = new TestResult(method);

        try
        {
          if (testObject is Node node)
          {
            _sceneTree.Root.AddChild(node);
          }

          _setupMethods.TryGetValue(method.DeclaringType, out var setupMethod);
          setupMethod?.Invoke(testObject, new object[] { });

          object obj = method.Invoke(testObject, new object[] { });

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

        action(testResult);
        await Task.Delay(1);
      }
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
        var testFixtureAttr = GetCustomAttribute<TestFixtureAttribute>(type);
        if (testFixtureAttr == null)
          continue;

        var constructor = FindConstructor(type, testFixtureAttr.Arguments);
        if (constructor == null)
          continue; // TODO really?

        object TestObjectCreator() => constructor.Invoke(testFixtureAttr.Arguments);

        var methods = type.GetMethods();
        foreach (var method in methods)
        {
          ScanMethod(type, method, TestObjectCreator);
        }
      }
    }

    private void ScanMethod(Type type, MethodInfo method, Func<object> testObjectCreator)
    {
      var testAttr = GetCustomAttribute<TestAttribute>(method);
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
      else if (testAttr != null)
      {
        _testMethods[method] = testObjectCreator();
      }
    }

    private ConstructorInfo? FindConstructor(Type type, object[] args)
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
      return Attribute.GetCustomAttribute(type, typeof(T), false) as T;
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
      this.TestMethod = testMethod;
      this.Exception = exception;
    }
  }
}