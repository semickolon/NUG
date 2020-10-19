using System.IO;
using System.Threading.Tasks;
using Godot;

namespace NUG.Internal
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
      using var writer = new StreamWriter(".nugout.txt");
      
      await testRunner.Run(res =>
      {
        var name = $"{res.TestMethod.DeclaringType!.Name}.{res.TestMethod.Name}";
        if (res.Exception != null)
        {
          writer.WriteLine($"❌  \u001B[31mFailed {name}\n{res.Exception.Message}\u001B[0m");
          OS.ExitCode = 1;
          throw res.Exception;
        }

        writer.WriteLine($"✔️  Passed {name}");
      });
    }
  }
}