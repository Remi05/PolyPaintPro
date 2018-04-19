using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PolyPaint.Utils
{
    class CustomStringContent: StringContent
    {
        public CustomStringContent(string name, string content):
           base (content)
        {
            Headers.ContentType = null;
            Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = name
            };
        }
    }
}
