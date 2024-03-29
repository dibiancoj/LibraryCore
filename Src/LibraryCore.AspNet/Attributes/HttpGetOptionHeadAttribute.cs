﻿using Microsoft.AspNetCore.Mvc.Routing;
using System.Collections.Immutable;

namespace LibraryCore.AspNet.Attributes;

/// <summary>
/// Allow either an httpet, head, or options is allowed. Useful with load balanced env's.
/// </summary>
public class HttpGetOptionHeadAttribute : HttpMethodAttribute
{
    private static IImmutableList<string> SupportedMethodTypes { get; } = ImmutableList.CreateRange([
        HttpMethod.Get.Method.ToUpper(),
        HttpMethod.Options.Method.ToUpper(),
        HttpMethod.Head.Method.ToUpper()
    ]);

    public HttpGetOptionHeadAttribute()
        : base(SupportedMethodTypes)
    {
    }

    public HttpGetOptionHeadAttribute(string template)
        : base(SupportedMethodTypes, template)
    {
        //template should be not be null in this overload. Throw like all the rest of the HttpMethodAttribute implementations by Microsoft.
        ArgumentNullException.ThrowIfNull(template);
    }
}
