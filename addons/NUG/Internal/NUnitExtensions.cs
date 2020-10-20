using System.Reflection;
using NUnit.Framework;

namespace NUG.Internal
{
  public static class NUnitExtensions
  {
    public static int GetCount(this RepeatAttribute repeatAttr)
    {
      return (int) GetInstanceField(repeatAttr, "_count");
    }
    
    private static object GetInstanceField<T>(T source, string fieldName)
    {
      var bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
      var field = typeof(T).GetField(fieldName, bindFlags);
      return field!.GetValue(source);
    }
  }
}