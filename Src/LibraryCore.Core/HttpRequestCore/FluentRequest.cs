using LibraryCore.Core.ExtensionMethods;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
using System.Net.Http.Json;
using static LibraryCore.Core.ContentType.ContentTypeLookup;

namespace LibraryCore.Core.HttpRequestCore;

public class FluentRequest
{
    public FluentRequest(HttpMethod httpMethodType, string url)
    {
        Message = new HttpRequestMessage(httpMethodType, url);
    }

    public FluentRequest(HttpMethod httpMethodType) :
        this(httpMethodType, string.Empty)
    {
    }

    public HttpRequestMessage Message { get; }

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

    public FluentRequest AddAcceptType(AcceptTypeEnum acceptType)
    {
        Message.Headers.Accept.Add(RetrieveAcceptType(acceptType));
        return this;
    }

    public FluentRequest AddJsonBody<T>(T model)
    {
        Message.Content = JsonContent.Create(model);
        return this;
    }

    public FluentRequest AddFormsUrlEncodedBody(IEnumerable<KeyValuePair<string, string>> parametersToAdd)
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

    /// <summary>
    /// Build up the file to be uploaded from a streamy which will be sent over the wire in an http request
    /// </summary>
    /// <param name="filesToUpload">Files to upload. Key is the file name value is the stream</param>
    /// <param name="parameterName">This is the parameter name in web api. If this doesn't match then the binding won't come through with any files</param>
    /// <returns>MultipartFormDataContent that contains the file to be uploaded in the http request</returns>
    private static MultipartFormDataContent BuildMultipartForm(string parameterName, IEnumerable<KeyValuePair<string, Stream>> filesToUpload)
    {
        var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));

        //portal svc has a few checks. we need file name to have quotes "jason.jpg" around it. .net default does not include quotes
        foreach (var fileStreamToAdd in filesToUpload)
        {
            //always reset / rewind the stream to the beginning incase there was some reads on it
            fileStreamToAdd.Value.Seek(0, SeekOrigin.Begin);

            content.Add(new StreamContent(fileStreamToAdd.Value), parameterName.SurroundWithQuotes(), fileStreamToAdd.Key.SurroundWithQuotes());
        }

        //return it
        return content;
    }

}

