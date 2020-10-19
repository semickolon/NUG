using System.Collections.Generic;
using System.Reflection;

namespace NUG.Internal
{
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