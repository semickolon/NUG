using Godot;
using System;
using NUG;

namespace NUG.Tests
{
  public class Scene : Node
  {
    public override async void _Ready()
    {
      var testRunner = new TestRunner(GetTree());
      
      await testRunner.Run(res =>
      {
        var name = $"{res.TestMethod.DeclaringType!.Name}.{res.TestMethod.Name}";
        if (res.Exception != null)
        {
          GD.Print($"Failed {name} : {res.Exception.Message}");
        }
        else
        {
          GD.Print($"Passed {name}");
        }
      });
      
      GetTree().Quit();
    }
  }
}