﻿using LibraryCore.Core.ExtensionMethods;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Globalization;
using System.Net.Http.Headers;
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
        if (Message.RequestUri == null)
        {
            throw new NullReferenceException("Null Request Uri");
        }

        Message.RequestUri = new Uri(QueryHelpers.AddQueryString(Message.RequestUri.ToString(), name, value), UriKind.RelativeOrAbsolute);
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

    public FluentRequest AddFileStreamBody(string fileName, byte[] fileBytes)
    {
        Message.Content = BuildMultipartForm(fileName, new MemoryStream(fileBytes));
        return this;
    }

    public FluentRequest AddFileStreamBody(string fileName, Stream fileStream)
    {
        Message.Content = BuildMultipartForm(fileName, fileStream);
        return this;
    }

    /// <summary>
    /// Build up the file to be uploaded from a streamy which will be sent over the wire in an http request
    /// </summary>
    /// <param name="fileName">file name for this file</param>
    /// <param name="fileStream">file bytes which contains the raw content of this file</param>
    /// <returns>MultipartFormDataContent that contains the file to be uploaded in the http request</returns>
    private static MultipartFormDataContent BuildMultipartForm(string fileName, Stream fileStream)
    {
        //portal svc has a few checks. we need file name to have quotes "jason.jpg" around it. .net default does not include quotes

        //always reset / rewind the stream to the beginning incase there was some reads on it
        fileStream.Seek(0, SeekOrigin.Begin);

        //create the start of the file upload
        var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture))
        {

            //add the content
            { new StreamContent(fileStream), fileName.SurroundWithQuotes(), fileName.SurroundWithQuotes() }
        };

        //return it
        return content;
    }

    /// <summary>
    /// No need to grab the http request message. Allow the conversion
    /// </summary>
    public static implicit operator HttpRequestMessage(FluentRequest request) => request.Message;

}

