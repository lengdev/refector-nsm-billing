//-----------------------------------------------------------------------
// <copyright file="AuthorizationHeaderHandler.cs" company="None">
//     Copyright (c) Allow to distribute this code.
// </copyright>
// <author>Asma Khalid</author>
//-----------------------------------------------------------------------
namespace NSMSOLUTION.Helper_Code.Common
{
    using NSMSOLUTION.Resources.Constants;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    /// <summary>
    /// Authorization for web API class.
    /// </summary>
    public class AuthorizationHeaderHandler : DelegatingHandler
    {
        #region Send method.

        /// <summary>
        /// Send method.
        /// </summary>
        /// <param name="request">Request parameter</param>
        /// <param name="cancellationToken">Cancellation token parameter</param>
        /// <returns>Return HTTP response.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AuthenticationHeaderValue authorization = request.Headers.Authorization;
            
            // 1. NEW: Check Bearer static token from Web.config
            if (authorization != null && "Bearer".Equals(authorization.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                string token = authorization.Parameter;
                string validToken = System.Configuration.ConfigurationManager.AppSettings["CustomerActivateApiStaticToken"];
                if (!string.IsNullOrWhiteSpace(token) && token.Equals(validToken, StringComparison.Ordinal))
                {
                    var identity = new GenericIdentity("StaticTokenUser");
                    SetPrincipal(new GenericPrincipal(identity, null));
                    return base.SendAsync(request, cancellationToken);
                }
            }

            // Initialization.
            IEnumerable<string> apiKeyHeaderValues;
            // Verification.
            if (request.Headers.TryGetValues(ApiInfo.API_KEY_HEADER, out apiKeyHeaderValues) &&
                authorization != null && !string.IsNullOrEmpty(authorization.Parameter))
            {
                var apiKeyHeaderValue = apiKeyHeaderValues.First();

                // Get the auth token
                string authToken = authorization.Parameter;

                // Decode the token from BASE64
                string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(authToken));

                // Extract username and password from decoded token
                string userName = decodedToken.Substring(0, decodedToken.IndexOf(":"));
                string password = decodedToken.Substring(decodedToken.IndexOf(":") + 1);

                // Verification.
                if (apiKeyHeaderValue.Equals(ApiInfo.API_KEY_VALUE) &&
                    userName.Equals(ApiInfo.USERNAME_VALUE) &&
                    password.Equals(ApiInfo.PASSWORD_VALUE))
                {
                    // Setting
                    var identity = new GenericIdentity(userName);
                    SetPrincipal(new GenericPrincipal(identity, null));
                }
            }

            // Info.
            return base.SendAsync(request, cancellationToken);
        }

        #endregion

        #region Set principal method.

        /// <summary>
        /// Set principal method.
        /// </summary>
        /// <param name="principal">Principal parameter</param>
        private static void SetPrincipal(IPrincipal principal)
        {
            // setting.
            Thread.CurrentPrincipal = principal;

            // Verification.
            if (HttpContext.Current != null)
            {
                // Setting.
                HttpContext.Current.User = principal;
            }
        }

        #endregion
    }
}