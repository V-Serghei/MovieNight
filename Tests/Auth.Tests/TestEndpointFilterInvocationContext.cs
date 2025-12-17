using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

public sealed class TestEndpointFilterInvocationContext : EndpointFilterInvocationContext
{
    private readonly HttpContext _http;
    private readonly List<object?> _args;

    public TestEndpointFilterInvocationContext(HttpContext httpContext, params object?[] args)
    {
        _http = httpContext;
        _args = new List<object?>(args);
    }

    public override HttpContext HttpContext => _http;

    public override IList<object?> Arguments => _args;

    public override T GetArgument<T>(int index) => (T)_args[index]!;
}