using Newtonsoft.Json;

namespace LibraryCore.JsonNet;

public class JsonToStreamResult : IDisposable
{
    internal JsonToStreamResult(MemoryStream memoryStream)
    {
        StreamToWriteInto = memoryStream;
        StreamWriterToUse = new StreamWriter(StreamToWriteInto);
        JsonTextWriterToUse = new JsonTextWriter(StreamWriterToUse);
    }

    internal MemoryStream StreamToWriteInto { get; }
    internal StreamWriter StreamWriterToUse { get; }
    internal JsonTextWriter JsonTextWriterToUse { get; }
    private bool Disposed { get; set; }
    
    /// <summary>
    /// This is only meant to be consumed once. Multiple consumes on the same stream is not supported.
    /// </summary>
    public MemoryStream ToStream()
    {
        StreamToWriteInto.Position = 0;
        return StreamToWriteInto;
    }

    public byte[] ToByteArray() => StreamToWriteInto.ToArray();

    #region Dispose Method

    /// <summary>
    /// Disposes My Object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose Overload. Ensures my database connection is closed
    /// </summary>
    private void Dispose(bool disposing)
    {
        if (!this.Disposed)
        {
            if (disposing)
            {
                JsonTextWriterToUse.Close();
                ((IDisposable)JsonTextWriterToUse).Dispose();
                StreamWriterToUse.Dispose();
                StreamToWriteInto.Dispose();

            }
        }
        Disposed = true;
    }

    #endregion

}
