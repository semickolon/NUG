using System;

namespace NUG.Internal
{
  public static class GeneralExtensions
  {
    public static void With<T>(this T t, Action<T> action)
    {
      action(t);
    }
  }
}