using System.Threading.Tasks;
using Godot;

namespace NUG
{
  public class CliTestRunner : SceneTree
  {
    public CliTestRunner()
    {
      RunAllTests().ContinueWith(_ => Quit());
    }

    private async Task RunAllTests()
    {
      var testRunner = new TestRunner(this);
      
      await testRunner.Run(res =>
      {
        var name = $"{res.TestMethod.DeclaringType!.Name}.{res.TestMethod.Name}";
        if (res.Exception != null)
        {
          GD.Print($"Failed {name} : {res.Exception.Message}");
          OS.ExitCode = 1;
          throw res.Exception;
        }

        GD.Print($"Passed {name}");
      });
    }
  }
}