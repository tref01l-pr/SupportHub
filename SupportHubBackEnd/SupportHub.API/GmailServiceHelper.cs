using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using SupportHub.Domain.Options;
using Microsoft.Extensions.Options;

namespace SupportHub.API;

public static class GmailServiceHelper
{



    /*private static readonly string[] Scopes = { GmailService.Scope.GmailSend };

    public async Task<GmailService> GetGmailServiceAsync()
    {
        var secrets = new ClientSecrets
        {
            ClientId = _пoogleApiOptions.ClientId,
            ClientSecret = _пoogleApiOptions.ClientSecret
        };

        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets,
            Scopes,
            "user",
            CancellationToken.None,
            new FileDataStore("token.json", true));

        return new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "MailBridgeSupportGmailApi",
        });
    }*/
    
    private static readonly string[] Scopes = { GmailService.Scope.MailGoogleCom };
    private static readonly string _applicationName = "Gmail API Application";

    private static readonly string path =
        @"C:\Users\prots\Desktop\ImapAndSmtpProject\MailBridgeSupport\MailBridgeSupportBackEnd\SupportHub.API\client_secret.json";
    
    private static readonly string folferPath =  @"C:\Users\prots\Desktop\ImapAndSmtpProject\MailBridgeSupport\MailBridgeSupportBackEnd\SupportHub.API";
    private static readonly string filePath = Path.Combine(folferPath, "APITokenCredentials");

    public static async Task<GmailService> GetAccessTokenAsync()
    {
        UserCredential userCredential;
        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            var asd = GoogleClientSecrets.Load(stream).Secrets;
            userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                asd,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(filePath, true));
        }

        GmailService service = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = userCredential,
            ApplicationName = _applicationName
        });

        return service;
    }
}