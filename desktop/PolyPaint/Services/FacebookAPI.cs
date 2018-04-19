using PolyPaint.Utils;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PolyPaint.Services
{
    public static class FacebookAPI
    {
        private static class Constants
        {
            public static readonly string FacebookAuthEndpoint = "https://www.facebook.com/v2.12/dialog/oauth?";
            public static readonly string FacebookClientIdParameter = "client_id=";
            public static readonly string FacebookRedirectUriParameter = "redirect_uri=";
            public static readonly string FacebookResponseTypeParameter = "response_type=";
            public static readonly string FacebookScopeParameter = "scope=";

            public static readonly string FacebookEndpoint = "https://graph.facebook.com";
            public static readonly string FacebookApiPath = "/me/photos?";
            public static readonly string FacebookMessageParameter = "message=";
            public static readonly string FacebookAccessTokenParameter = "access_token=";

            public static readonly string FacebookFileNamePrefix = "PolyPaintPro-";
        }
        private static string FacebookAppId { get { return ConfigurationManager.AppSettings.Get("FacebookAppId"); } }
        private static string FacebookRedirectURI { get { return ConfigurationManager.AppSettings.Get("FacebookRedirectURI"); } }
        private static string FacebookResponseType { get { return ConfigurationManager.AppSettings.Get("FacebookResponseType"); } }
        private static string FacebookScope { get { return ConfigurationManager.AppSettings.Get("FacebookScope"); } }
        public static string FacebookAuthenticationUri
        {
            get
            {
                return Constants.FacebookAuthEndpoint +
                    Constants.FacebookClientIdParameter + FacebookAppId +
                    "&" + Constants.FacebookRedirectUriParameter + FacebookRedirectURI +
                    "&" + Constants.FacebookResponseTypeParameter + FacebookResponseType +
                    "&" + Constants.FacebookScopeParameter + FacebookScope;
            }
        }

        private static string FacebookFileName { get => Constants.FacebookFileNamePrefix + DateTime.Now.Ticks; }

        public async static Task<string> ShareImage(MemoryStream imageStream, string facebookAccessToken, string caption)
        {
            using (HttpClient client = new HttpClient())
            {
                var imageData = imageStream.ToArray();

                HttpContent imageContent = new ByteArrayContentFlexibleContentType(imageData);
                imageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = FacebookFileName
                };
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                var postContent = new MultipartFormDataContent()
                    {
                        imageContent
                    };

                string facebookQuery = Constants.FacebookEndpoint + Constants.FacebookApiPath +
                    Constants.FacebookMessageParameter + caption +
                    "&" + Constants.FacebookAccessTokenParameter + facebookAccessToken;

                client.DefaultRequestHeaders.ExpectContinue = false;
                var result = await client.PostAsync(facebookQuery, postContent);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return "Your drawing was successfully shared on facebook.";
                }
                return "An error occured while sharing your drawing.";
            }
        }
    }
}
