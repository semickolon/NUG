using System;

namespace NUG.Internal
{
  public static class GeneralExtensions
  {
    public static void With<T>(this T t, Action<T> action)
    {
      action(t);
    }
    
    public static R Map<T, R>(this T t, Func<T, R> func)
    {
      return func(t);
    }
  }
}