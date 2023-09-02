using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Serilog;
using ConsoleAppSignInADB2C.Inputs;
using ConsoleAppSignInADB2C.Security;

Console.WriteLine("***** Autenticando usuarios com Azure AD B2C *****");
Console.WriteLine();

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("azureadb2c-logs.tmp")
    .CreateLogger();

var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile($"appsettings.json");
var configuration = builder.Build();

string clientId = configuration["ADB2C:ConsoleAppId"]!;
string tenantName = configuration["ADB2C:TenantName"]!;
string tenant = $"{tenantName}.onmicrosoft.com";
string azureAdB2CHostname = $"{tenantName}.b2clogin.com";
string redirectUri = configuration["ADB2C:RedirectUri"]!;
string authorityBase = $"https://{azureAdB2CHostname}/tfp/{tenant}/";

string continuar;
do
{
    string userFlow = InputHelper.GetUserFlow(configuration);
    string authoritySignUpSignIn = $"{authorityBase}{userFlow}";

    logger.Information($"Exibindo a tela de login com {nameof(PublicClientApplicationBuilder)} | User Flow: {userFlow}");
    var app = PublicClientApplicationBuilder
        .Create(clientId)
        .WithB2CAuthority(authoritySignUpSignIn)
        .WithRedirectUri(redirectUri)
        .Build(); 
    string[] scopes = { "openid offline_access" };
    var result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();

    if (result.IdToken is not null)
    {
        logger.Information($"Id Token: {result.IdToken}");
        logger.Information($"*** Algumas Claims do Usuario ***");
        var infoIdToken = TokenHelper.ParseIdToken(result.IdToken);
        logger.Information($"Nome = {infoIdToken["given_name"]}");
        logger.Information($"Sobrenome = {infoIdToken["family_name"]}");
        logger.Information($"Cidade = {infoIdToken["city"]}");
        logger.Information($"Pais = {infoIdToken["country"]}");
    }
    else
    {
        logger.Error($"O Id Token nao foi gerado!");
    }

    continuar = InputHelper.GetAnswerContinue();
} while (continuar == "Sim");