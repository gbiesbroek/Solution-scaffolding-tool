namespace Scaffold.Cli.Menu;

public class NavigationContext
{
    public bool ShouldExit { get; private set; }
    public bool ShouldGoBack { get; private set; }

    public void Exit() => ShouldExit = true;
    public void GoBack() => ShouldGoBack = true;
}
