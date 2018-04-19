using System;
using System.Text.RegularExpressions;
using System.Web;

namespace Slofth.Firebase.Utils
{
    internal class UrlBuilder
    {
        public string Query => Builder.Query;
        public string Path => Builder.Path;
        public string Fragment => Builder.Fragment;
        public string Url => Builder.ToString();

        private UriBuilder Builder { get; set; }

        private UrlBuilder(string baseUrl)
        {
            Builder = new UriBuilder(baseUrl);
        }

        static public UrlBuilder Create(string baseUrl)
        {
            return new UrlBuilder(baseUrl);
        }

        static public UrlBuilder Create(Uri baseUri)
        {
            return new UrlBuilder(baseUri.OriginalString);
        }

        public UrlBuilder AddParam(string name, string value)
        {
            var query = HttpUtility.ParseQueryString(Builder.Query);
            query[name] = value;
            Builder.Query = query.ToString();

            return this;
        }

        public UrlBuilder AddParam(string name, int value)
        {
            return AddParam(name, value.ToString());
        }

        public UrlBuilder AppendToPath(string path)
        {
            path = $"{Builder.Path}/{path}";
            path = Regex.Replace(path, "\\/+", "/");
            path = Regex.Replace(path, "\\/+$", "");
            Builder.Path = path;
            return this;
        }

        public UrlBuilder Copy()
        {
            return new UrlBuilder(Builder.Uri.ToString());
        }

        public override string ToString()
        {
            return Builder.ToString();
        }
    }
}
