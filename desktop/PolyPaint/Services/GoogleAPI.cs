using Newtonsoft.Json.Linq;
using PolyPaint.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PolyPaint.Services
{
    public static class GoogleAPI
    {
        private static class Constants
        {
            public static readonly string GoogleDriveApiPath = "/upload/drive/v3/files?uploadType=multipart";
            public static readonly string GoogleDriveFileNamePrefix = "PolyPaintPro-";

            public static readonly string GoogleApiEndpoint = "https://www.googleapis.com";
            public static readonly string GoogleUrlPath = "/oauth2/v4/token";
            public static readonly string GoogleCodeParameter = "code";
            public static readonly string GoogleClientIdParameter = "client_id";
            public static readonly string GoogleClientSecretParameter = "client_secret";
            public static readonly string GoogleRedirectUriParameter = "redirect_uri";
            public static readonly string GoogleGrantTypeParameter = "grant_type";

            public static readonly string GoogleEndpoint = "https://accounts.google.com/o/oauth2/v2/auth?";
            public static readonly string GoogleScopeParameter = "scope=";
            public static readonly string GoogleClientIdAuthParameter = "client_id=";
            public static readonly string GoogleRedirectUriAuthParameter = "redirect_uri=";
            public static readonly string GoogleResponseTypeParameter = "response_type=";
        }

        private static string GoogleAppId { get => ConfigurationManager.AppSettings.Get("GoogleAppId"); }
        private static string GoogleRedirectURI { get => ConfigurationManager.AppSettings.Get("GoogleRedirectURI"); }
        private static string GoogleClientSecret { get => ConfigurationManager.AppSettings.Get("GoogleClientSecret"); }
        private static string GoogleGrantType { get => ConfigurationManager.AppSettings.Get("GoogleGrantType"); }
        private static string GoogleDriveFileName { get => Constants.GoogleDriveFileNamePrefix + DateTime.Now.Ticks; }
        private static string GoogleResponseType { get { return ConfigurationManager.AppSettings.Get("GoogleResponseType"); } }
        private static string GoogleScope { get { return ConfigurationManager.AppSettings.Get("GoogleScope"); } }
        public static string GoogleAuthenticationUri
        {
            get
            {
                return Constants.GoogleEndpoint +
                    Constants.GoogleScopeParameter + GoogleScope +
                    "&" + Constants.GoogleResponseTypeParameter + GoogleResponseType +
                    "&" + Constants.GoogleRedirectUriAuthParameter + GoogleRedirectURI +
                    "&" + Constants.GoogleClientIdAuthParameter + GoogleAppId;
            }
        }

        public async static Task<string> GetGoogleAccessToken(string code)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Constants.GoogleApiEndpoint);
                HttpContent content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(Constants.GoogleCodeParameter, code),
                    new KeyValuePair<string, string>(Constants.GoogleClientIdParameter, GoogleAppId),
                    new KeyValuePair<string, string>(Constants.GoogleClientSecretParameter, GoogleClientSecret),
                    new KeyValuePair<string, string>(Constants.GoogleRedirectUriParameter, GoogleRedirectURI),
                    new KeyValuePair<string, string>(Constants.GoogleGrantTypeParameter, GoogleGrantType)
                });

                var result = await client.PostAsync(Constants.GoogleUrlPath, content);
                string resultContent = await result.Content.ReadAsStringAsync();

                return (string)JObject.Parse(resultContent)["access_token"];
            }
        }
        
        public async static Task<string> SaveOnGoogleDrive(MemoryStream image, string extension, string GoogleAuthorizationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                var imageData = image.ToArray();
                
                var imagePostContent = new ByteArrayContentFlexibleContentType(imageData)
                {
                    ContentType = "image/" + extension
                };
                string name = GoogleDriveFileName + "." + extension;
                var metadataPostContent = new StringContent(
                    "{ name:\"" + name  + "\" }", 
                    Encoding.UTF8, "application/json"
                    );

                var postContent = new MultipartContent
                {
                    metadataPostContent,
                    imagePostContent
                };

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GoogleAuthorizationToken);
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.BaseAddress = new Uri(Constants.GoogleApiEndpoint);
                var result = await client.PostAsync(Constants.GoogleDriveApiPath, postContent);

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return "Your drawing was successfully saved to google drive.";
                }
                return "An error occured while exporting your drawing to Google Drive.";
            }
        }
    }
}
