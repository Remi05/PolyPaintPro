using PolyPaint.Services.Auth;
using PolyPaint.Services.Drawing;
using PolyPaint.Services.Logger;
using PolyPaint.Services.Social;
using PolyPaint.Utils;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace PolyPaint.Services.Email
{
    class MailService : IMailService
    {
        private class Constants
        {
            public const string MailApiHost = "https://api.mailgun.net";
            public const string MailApiPath = "/v3/mg.polypaintpro.club/messages";
            public const string SmtpHost = "smtp.mailgun.org";
            public const string EmailAddress = "PolyPaint Pro noreply@polypaintpro.club";
            public const string MailGunApiKey = "key-1b19c3e7afa1effc29f23e49d00007f0";
            public const string EmailSubject = "One of your drawings has been removed";
            public const string EmailBody = @"
Hi {0},

Your obscene behavior has caused your drawing to be reported and removed. 
If you feel like this is unjustified, you can report the issue to an administrator at thisisnotanemail@polypaintpro.club.

Issue number : {1}

Have a great day, 
The PolyPaint Pro team
";
        }

        private IAuthenticationService AuthService { get; set; }
        private IProfileService ProfileService { get; set; }
        private IDrawingService DrawingService { get; set; }
        private ILogger Logger { get; set; }

        public MailService(IAuthenticationService authService, IProfileService profileService, IDrawingService drawingService, ILogger logger)
        {
            AuthService = authService;
            Logger = logger;
            ProfileService = profileService;
            DrawingService = drawingService;
        }

        public async void EmailCurrentUserOnImageBanned()
        {
            if (!AuthService.IsLoggedIn || string.IsNullOrWhiteSpace(AuthService.CurrentUser.Email))
                return;

            var ids = await ProfileService.GetDrawingsIdsNoFilter(AuthService.CurrentUser.Id);
            foreach (var id in ids)
            {
                var drawingInfo = await DrawingService.GetDrawingInfo(id);
                if (drawingInfo == null
                    || !drawingInfo.IsBanned 
                    || drawingInfo.EmailSent
                    || drawingInfo.Owner != AuthService.CurrentUser.Id)
                    continue;

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Constants.MailApiHost);
                    var content = new MultipartFormDataContent
                    {
                        new CustomStringContent("from", Constants.EmailAddress),
                        new CustomStringContent("to", AuthService.CurrentUser.Email),
                        new CustomStringContent("subject", Constants.EmailSubject),
                        new CustomStringContent("text", string.Format(Constants.EmailBody,
                                                               AuthService.CurrentUser.DisplayName,
                                                               drawingInfo.Id))
                    };

                    client.DefaultRequestHeaders.ExpectContinue = false;
                    client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue(
                                "Basic",
                                Convert.ToBase64String(Encoding.UTF8.GetBytes("api:" + Constants.MailGunApiKey)));

                    var response = await client.PostAsync(Constants.MailApiPath, content);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        await DrawingService.TagDrawingInfoAsEmailSent(drawingInfo.Id);
                        Logger.Info("Report email sent for image " + drawingInfo.Id + " to user " + AuthService.CurrentUser.Id);
                    }
                    else
                    {
                        Logger.Error("Error while sending the report email: \n" + response.Content);
                    }
                }
            }
        }
    }
}
