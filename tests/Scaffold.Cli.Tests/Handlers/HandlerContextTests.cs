using Scaffold.Cli.Handlers;

namespace Scaffold.Cli.Tests.Handlers;

public class HandlerContextTests
{
    [Fact]
    public void NewContext_WorkingDirectory_MatchesConstructorArg()
    {
        var ctx = new HandlerContext("C:\\work");
        Assert.Equal("C:\\work", ctx.WorkingDirectory);
    }

    [Fact]
    public void NewContext_ShouldGoBackIsFalse()
    {
        var ctx = new HandlerContext("C:\\work");
        Assert.False(ctx.ShouldGoBack);
    }

    [Fact]
    public void GoBack_SetsShouldGoBackTrue()
    {
        var ctx = new HandlerContext("C:\\work");
        ctx.GoBack();
        Assert.True(ctx.ShouldGoBack);
    }
}
