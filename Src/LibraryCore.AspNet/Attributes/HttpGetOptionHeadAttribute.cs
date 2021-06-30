using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Immutable;
using System.Net.Http;

namespace LibraryCore.AspNet.Attributes
{
    /// <summary>
    /// Allow either an http get or an http option (for the load balancer)
    /// </summary>
    public class HttpGetOptionHeadAttribute : HttpMethodAttribute
    {
        private static IImmutableList<string> SupportedMethodTypes { get; } = new[]
        {
            HttpMethod.Get.Method.ToUpper(),
            HttpMethod.Options.Method.ToUpper(),
            HttpMethod.Head.Method.ToUpper()
        }.ToImmutableList();

        public HttpGetOptionHeadAttribute()
            : base(SupportedMethodTypes)
        {
        }

        public HttpGetOptionHeadAttribute(string template)
            : base(SupportedMethodTypes, template)
        {
            //template should be not be null in this overload. Throw like all the rest of the HttpMethodAttribute implementations by Microsoft.
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }
        }
    }
}
