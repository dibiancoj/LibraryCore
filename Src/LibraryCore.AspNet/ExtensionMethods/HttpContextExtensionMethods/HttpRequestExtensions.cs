using Microsoft.AspNetCore.Http;

namespace LibraryCore.AspNet.ExtensionMethods.HttpContextExtensionMethods;

public static class HttpRequestExtensions
{
    //header collection doesn't throw when it doesn't exist.
    public static bool IsAjaxRequest(this HttpRequest request) => request.Headers["X-Requested-With"] == "XMLHttpRequest";
}

