using System.Net.Http;
using System.Net.Http.Headers;

namespace PolyPaint.Utils
{
    class ByteArrayContentFlexibleContentType: ByteArrayContent
    {
        private string contentType;
        public string ContentType
        {
            get => contentType;
            set
            {
                Headers.ContentType = new MediaTypeHeaderValue(value);
                contentType = value;
            }
        }

        public ByteArrayContentFlexibleContentType(byte[] array): base(array) { }
    }
}
