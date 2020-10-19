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
        if (res.Exception != null)
        {
          GD.Print($"Failed {res.TestMethod.Name} : {res.Exception.Message}");
        }
        else
        {
          GD.Print($"Passed {res.TestMethod.Name}");
        }
      });
      
      GetTree().Quit();
    }
  }
}