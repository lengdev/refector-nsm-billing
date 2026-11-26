
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using NSMSOLUTION.Provider;
using Owin;
using System;

[assembly: OwinStartup(typeof(Startup))]

public class Startup
{
    public const string ApplicationCookieAuthenticationType =
        "ApplicationCookie";

    public void Configuration(IAppBuilder app)
    {
        // Keep your existing CORS configuration.
        app.UseCors(CorsOptions.AllowAll);

        // 1. MVC browser authentication.
        // This cookie is separate from the OAuth Bearer token.
        app.UseCookieAuthentication(
            new CookieAuthenticationOptions
            {
                AuthenticationType =
                    ApplicationCookieAuthenticationType,

                CookieName = ".NSM.Application",

                CookiePath = "/",

                CookieHttpOnly = true,

                // SameAsRequest supports local HTTP development.
                // Use Always in production when the site is HTTPS-only.
                CookieSecure = CookieSecureOption.SameAsRequest,

                ExpireTimeSpan = TimeSpan.FromDays(30),

                SlidingExpiration = true
            });

        // 2. Existing OAuth token server and Bearer authentication.
        ConfigureOAuth(app);

        // 3. Existing SignalR configuration.
        app.MapSignalR();

        // Run OWIN authentication at the ASP.NET authentication stage.
        app.UseStageMarker(PipelineStage.Authenticate);
    }

    private void ConfigureOAuth(IAppBuilder app)
    {
        var options = new OAuthAuthorizationServerOptions
        {
            AllowInsecureHttp = true, // Development only. Disable in production.
            TokenEndpointPath = new PathString("/token"),
            AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
            Provider = new AuthorizationServerProvider(),
            RefreshTokenProvider = new RefreshTokenProvider(),
            AuthenticationType = "Bearer"
        };

        app.UseOAuthAuthorizationServer(options);

        app.UseOAuthBearerAuthentication(
            new OAuthBearerAuthenticationOptions
            {
                AuthenticationType = "Bearer"
            });
    }
}
