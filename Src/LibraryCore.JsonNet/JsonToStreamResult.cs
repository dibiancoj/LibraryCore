using Newtonsoft.Json;

namespace LibraryCore.JsonNet
{
    public class JsonToStreamResult : IDisposable
    {
        internal JsonToStreamResult()
        {
            StreamToWriteInto = new MemoryStream();
            StreamWriterToUse = new StreamWriter(StreamToWriteInto);
            JsonTextWriterToUse = new JsonTextWriter(StreamWriterToUse);
        }

        private MemoryStream StreamToWriteInto { get; }
        private StreamWriter StreamWriterToUse { get; }
        private JsonTextWriter JsonTextWriterToUse { get; }
        private bool Disposed { get; set; }

        public MemoryStream RewindAndConsumeStream()
        {
            StreamToWriteInto.Position = 0;
            return StreamToWriteInto;
        }

        public byte[] ToByteArray() => RewindAndConsumeStream().ToArray();

        public static JsonToStreamResult SerializeToStream<T>(T modelToSerialize, JsonSerializer jsonSerializer)
        {
            var result = new JsonToStreamResult();

            jsonSerializer.Serialize(result.JsonTextWriterToUse, modelToSerialize);
            result.JsonTextWriterToUse.Flush();
            result.StreamWriterToUse.Flush();

            return result;
        }

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
}
