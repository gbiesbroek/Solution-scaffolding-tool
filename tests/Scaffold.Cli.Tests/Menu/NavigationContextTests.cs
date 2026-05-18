using Scaffold.Cli.Menu;

namespace Scaffold.Cli.Tests.Menu;

public class NavigationContextTests
{
    [Fact]
    public void NewContext_BothFlagsFalse()
    {
        var ctx = new NavigationContext();
        Assert.False(ctx.ShouldExit);
        Assert.False(ctx.ShouldGoBack);
    }

    [Fact]
    public void Exit_SetsExitFlag()
    {
        var ctx = new NavigationContext();
        ctx.Exit();
        Assert.True(ctx.ShouldExit);
        Assert.False(ctx.ShouldGoBack);
    }

    [Fact]
    public void GoBack_SetsGoBackFlag()
    {
        var ctx = new NavigationContext();
        ctx.GoBack();
        Assert.False(ctx.ShouldExit);
        Assert.True(ctx.ShouldGoBack);
    }
}
