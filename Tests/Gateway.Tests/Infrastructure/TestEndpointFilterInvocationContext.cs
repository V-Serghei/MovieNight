using Microsoft.AspNetCore.Http;

namespace Gateway.Tests.Infrastructure;

file sealed class TestEndpointFilterInvocationContext : EndpointFilterInvocationContext
{
    public override HttpContext HttpContext { get; }
    public override object?[] Arguments { get; }

    public TestEndpointFilterInvocationContext(HttpContext httpContext, object?[]? arguments = null)
    {
        HttpContext = httpContext;
        Arguments = arguments ?? Array.Empty<object?>();
    }

    public override T GetArgument<T>(int index)
    {
        return (T)Arguments[index]!;
    }
}