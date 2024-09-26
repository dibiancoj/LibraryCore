using System.Text;

namespace LibraryCore.Core.Authentication;

/// <summary>
/// Utilities for basic authentication
/// </summary>
public class BasicAuthentication
{
    /// <summary>
    /// Convert a user name and password to basic authentication
    /// </summary>
    /// <param name="userName">user name</param>
    /// <param name="password">password</param>
    /// <returns></returns>
    public static string ToBasicAuthenticationValue(string userName, string password) => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));

    /// <summary>
    /// Convert the encoded basic header value back to client id and secret. This is mainly for debugging and things you might need this for.
    /// </summary>
    /// <param name="headerValue">The header value to decode back to a client id and secret</param>
    /// <returns>Value Tuple with client and secret</returns>
    public static (string ClientId, string ClientSecret) DecodeBasicAuthenticationHeaderValue(string headerValue)
    {
        var temp = Encoding.UTF8.GetString(Convert.FromBase64String(headerValue)).AsSpan();

        var indexOfColon = temp.IndexOf(':');

        if (indexOfColon == -1)
        {
            throw new Exception("Invalid basic authentication header value. Value is missing the colon.");
        }

        return (temp[..indexOfColon].ToString(), temp[(indexOfColon + 1)..].ToString());
    }

}
