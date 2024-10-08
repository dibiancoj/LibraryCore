﻿using LibraryCore.Core.Authentication;
using LibraryCore.Shared;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using static LibraryCore.ApiClient.ContentTypeLookup;

namespace LibraryCore.ApiClient;

public class FluentRequest(HttpMethod httpMethodType, string url)
{
    public FluentRequest(HttpMethod httpMethodType) :
        this(httpMethodType, string.Empty)
    {
    }

    public HttpRequestMessage Message { get; } = new HttpRequestMessage(httpMethodType, url);

    /// <summary>
    /// No need to grab the http request message. Allow the conversion
    /// </summary>
    public static implicit operator HttpRequestMessage(FluentRequest request) => request.Message;

    public FluentRequest AddHeader(string headerName, string headerValue)
    {
        Message.Headers.Add(headerName, headerValue);
        return this;
    }

    public FluentRequest AddHeaders(IEnumerable<KeyValuePair<string, string>> headers)
    {
        foreach (var header in headers)
        {
            Message.Headers.Add(header.Key, header.Value);
        }

        return this;
    }

    public FluentRequest AddQueryString(string name, string value)
    {
        Message.RequestUri = new Uri(QueryHelpers.AddQueryString(Message.RequestUri!.ToString(), name, value), UriKind.RelativeOrAbsolute);
        return this;
    }

    public FluentRequest AddQueryStrings(IDictionary<string, string?> queryStringsToAdd)
    {
        Message.RequestUri = new Uri(QueryHelpers.AddQueryString(Message.RequestUri!.ToString(), queryStringsToAdd), UriKind.RelativeOrAbsolute);
        return this;
    }

    public FluentRequest AddUrlPath(string pathToAdd)
    {
        var builder = new UriBuilder(Message.RequestUri!);

        builder.Path = $"{builder.Path.TrimEnd('/')}/{pathToAdd}";

        Message.RequestUri = builder.Uri;

        return this;
    }

    public FluentRequest AddUrlPaths(params string[] pathsToAdd)
    {
        foreach (var pathToAdd in pathsToAdd)
        {
            AddUrlPath(pathToAdd);
        }

        return this;
    }

    public FluentRequest AddAcceptType(AcceptTypeEnum acceptType)
    {
        Message.Headers.Accept.Add(RetrieveAcceptType(acceptType));
        return this;
    }

    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccessUseOverload)]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
    public FluentRequest AddJsonBody<T>(T model, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        Message.Content = JsonContent.Create(model, options: jsonSerializerOptions);
        return this;
    }

    public FluentRequest AddJsonBody<T>(T model, JsonTypeInfo<T> jsonSerializerOptions)
    {
        Message.Content = JsonContent.Create(model, jsonSerializerOptions);
        return this;
    }

    public FluentRequest AddFormsUrlEncodedBody(params KeyValuePair<string, string>[] parametersToAdd)
    {
        Message.Content = new FormUrlEncodedContent(parametersToAdd);
        return this;
    }

    public FluentRequest AddFileStreamBody(string parameterName, params KeyValuePair<string, byte[]>[] filesToUpload)
    {
        Message.Content = BuildMultipartForm(parameterName, filesToUpload.Select(x => new KeyValuePair<string, Stream>(x.Key, new MemoryStream(x.Value))).ToList());
        return this;
    }

    public FluentRequest AddFileStreamBody(string parameterName, params KeyValuePair<string, Stream>[] filesToUpload)
    {
        Message.Content = BuildMultipartForm(parameterName, filesToUpload);
        return this;
    }

    public FluentRequest AddBasicAuthentication(string userName, string password)
    {
        Message.Headers.Authorization = new AuthenticationHeaderValue("Basic", BasicAuthentication.ToBasicAuthenticationValue(userName, password));
        return this;
    }

    /// <summary>
    /// Add an authenticated token like "bearer" or "token"
    /// </summary>
    /// <param name="scheme">bearer or token or the scheme used in auth</param>
    /// <param name="token">token value</param>
    public FluentRequest AddAuthenticationHeader(string scheme, string token)
    {
        Message.Headers.Authorization = new AuthenticationHeaderValue(scheme, token);
        return this;
    }

    /// <summary>
    /// Build up the file to be uploaded from a streamy which will be sent over the wire in an http request
    /// </summary>
    /// <param name="filesToUpload">Files to upload. Key is the file name value is the stream</param>
    /// <param name="parameterName">This is the parameter name in web api. If this doesn't match then the binding won't come through with any files</param>
    /// <returns>MultipartFormDataContent that contains the file to be uploaded in the http request</returns>
    private static MultipartFormDataContent BuildMultipartForm(string parameterName, IEnumerable<KeyValuePair<string, Stream>> filesToUpload)
    {
        var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));

        static string SurroundWithQuotes(string value)
        {
            const string stringLiteral = "\"";

            return $"{stringLiteral}{value}{stringLiteral}";
        }

        //portal svc has a few checks. we need file name to have quotes "jason.jpg" around it. .net default does not include quotes
        foreach (var fileStreamToAdd in filesToUpload)
        {
            //always reset / rewind the stream to the beginning incase there was some reads on it
            fileStreamToAdd.Value.Seek(0, SeekOrigin.Begin);

            content.Add(new StreamContent(fileStreamToAdd.Value), SurroundWithQuotes(parameterName), SurroundWithQuotes(fileStreamToAdd.Key));
        }

        //return it
        return content;
    }

}

