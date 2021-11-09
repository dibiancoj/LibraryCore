using System;

namespace LibraryCore.Core.UrlUtilities;

public static class UrlUtility
{
    /// <summary>
    /// If the uri to transform is a relative uri then it will transform the uri into an absolute uri. If its already absolute then it will return the uri passed in
    /// </summary>
    /// <param name="urlToTransform">Uri to make absolute</param>
    /// <param name="baseUrlWebSite">Uri of the web site that is the base address.</param>
    /// <returns>Absolute Uri</returns>
    public static Uri MakeRelativeUriAbsolute(string urlToTransform, Uri baseUrlWebSite)
    {
        return MakeRelativeUriAbsolute(new Uri(urlToTransform, UriKind.RelativeOrAbsolute), baseUrlWebSite);
    }

    /// <summary>
    /// If the uri to transform is a relative uri then it will transform the uri into an absolute uri. If its already absolute then it will return the uri passed in
    /// </summary>
    /// <param name="urlToTransform">Uri to make absolute</param>
    /// <param name="baseUrlWebSite">Uri of the web site that is the base address.</param>
    /// <returns>Absolute Uri</returns>
    public static Uri MakeRelativeUriAbsolute(Uri urlToTransform, Uri baseUrlWebSite)
    {
        //uri's don't handle www. as an absolute value. We want to treat that as absolute since we don't know what that value is.
        return urlToTransform.IsAbsoluteUri || urlToTransform.ToString().StartsWith("www", StringComparison.OrdinalIgnoreCase) ?
            urlToTransform :
            new Uri(baseUrlWebSite, urlToTransform);
    }

    /// <summary>
    /// Is this url an absolute uri...with a few additional checks
    /// </summary>
    /// <param name="uriToInspect">uri to inspect</param>
    /// <returns>True if it's an asbolute uri</returns>
    public static bool IsAbsoluteUri(string uriToInspect)
    {
        //"/test/doctor" //this code in linux is failing. So we need a special clause for it in the code
        return !uriToInspect.StartsWith("/", StringComparison.Ordinal) && (Uri.TryCreate(uriToInspect, UriKind.Absolute, out _) || uriToInspect.StartsWith("www", StringComparison.OrdinalIgnoreCase));
    }
}