
using MultipartDataMediaFormatter;
using MVC;
using NSMSOLUTION;
using NSMSOLUTION.Helper_Code.Common;
using NSMSOLUTION.Models.ApprovalUtility;
using NSMSOLUTION.Repositories;
using NSMSOLUTION.Services;

using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using Unity;
using Unity.AspNet.Mvc;

namespace NewInventorySystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static IUnityContainer _container;

        // =========================================================
        // APPLICATION START
        // =========================================================

        protected void Application_Start()
        {
            // =====================================================
            // 1. ANTI-FORGERY CONFIGURATION
            // =====================================================

            /*
             * Always use the same stable user identifier.
             *
             * AuthorizationServerProvider MUST include:
             *
             * new Claim(ClaimTypes.NameIdentifier, userId)
             *
             * Do not use ClaimTypes.Name as the anti-forgery
             * identifier because usernames can change.
             *
             * This setting must be configured before MVC
             * generates or validates anti-forgery tokens.
             */

            AntiForgeryConfig.UniqueClaimTypeIdentifier =
                ClaimTypes.NameIdentifier;

            // =====================================================
            // 2. REGISTER MVC AREAS
            // =====================================================

            AreaRegistration.RegisterAllAreas();

            // =====================================================
            // 3. WEB API CONFIGURATION
            // =====================================================

            GlobalConfiguration.Configure(WebApiConfig.Register);

            // =====================================================
            // 4. MVC GLOBAL FILTERS
            // =====================================================

            FilterConfig.RegisterGlobalFilters(
                GlobalFilters.Filters
            );

            // =====================================================
            // 5. ROUTING
            // =====================================================

            RouteConfig.RegisterRoutes(
                RouteTable.Routes
            );

            // =====================================================
            // 6. BUNDLES
            // =====================================================

            BundleConfig.RegisterBundles(
                BundleTable.Bundles
            );

            // =====================================================
            // 7. API AUTHORIZATION HANDLER
            // =====================================================

            GlobalConfiguration.Configuration.MessageHandlers.Add(
                new AuthorizationHeaderHandler()
            );

            // =====================================================
            // 8. MULTIPART FORMATTER
            // =====================================================

            GlobalConfiguration.Configuration.Formatters.Add(
                new FormMultipartEncodedMediaTypeFormatter()
            );

            // =====================================================
            // 9. UNITY DEPENDENCY INJECTION
            // =====================================================

            _container = new UnityContainer();

            // Approval service
            _container.RegisterType<
                IApprovalService,
                ApprovalService
            >();

            // Repositories
            _container.RegisterType<
                ITranRepository,
                TranRepository
            >();

            // Application services
            _container.RegisterType<
                ITelegramNotifier,
                TelegramNotifier
            >();

            _container.RegisterType<
                IPdfGenerator,
                PdfGenerator
            >();

            // =====================================================
            // 10. WEB API DEPENDENCY RESOLVER
            // =====================================================

            GlobalConfiguration.Configuration.DependencyResolver =
                new UnityResolver(_container);

            // =====================================================
            // 11. MVC DEPENDENCY RESOLVER
            // =====================================================

            DependencyResolver.SetResolver(
                new UnityDependencyResolver(_container)
            );
        }

        // =========================================================
        // AUTHENTICATION DIAGNOSTIC
        // =========================================================

        /*
         * Diagnostic only.
         *
         * Inspect the identity established for the current request.
         * Do not modify or manually create authentication identities
         * here. OWIN authentication middleware should establish them.
         *
         * Remove or disable detailed diagnostics after troubleshooting.
         */

        protected void Application_PostAuthenticateRequest(
            object sender,
            EventArgs e)
        {
            try
            {
                if (Context == null ||
                    Context.User == null ||
                    Context.User.Identity == null)
                {
                    return;
                }

                var identity =
                    Context.User.Identity as ClaimsIdentity;

                if (identity == null ||
                    !identity.IsAuthenticated)
                {
                    return;
                }

                var nameIdentifier =
                    identity.FindFirst(ClaimTypes.NameIdentifier);

                var name =
                    identity.FindFirst(ClaimTypes.Name);

                var userId =
                    identity.FindFirst("UserID");

                Debug.WriteLine("------------------------------------");
                Debug.WriteLine("AUTHENTICATION DIAGNOSTIC");
                Debug.WriteLine("------------------------------------");

                Debug.WriteLine(
                    "Authenticated: " + identity.IsAuthenticated
                );

                Debug.WriteLine(
                    "AuthenticationType: " +
                    identity.AuthenticationType
                );

                Debug.WriteLine(
                    "Name: " +
                    (name != null ? name.Value : "MISSING")
                );

                Debug.WriteLine(
                    "NameIdentifier: " +
                    (nameIdentifier != null
                        ? nameIdentifier.Value
                        : "MISSING")
                );

                Debug.WriteLine(
                    "UserID: " +
                    (userId != null ? userId.Value : "MISSING")
                );

                if (nameIdentifier == null ||
                    string.IsNullOrWhiteSpace(nameIdentifier.Value))
                {
                    Debug.WriteLine(
                        "WARNING: NameIdentifier is missing. " +
                        "Anti-forgery validation may fail."
                    );
                }

                Debug.WriteLine("------------------------------------");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "Authentication diagnostic error: " + ex.Message
                );
            }
        }

        // =========================================================
        // APPLICATION END
        // =========================================================

        protected void Application_End(object sender, EventArgs e)
        {
            // Close browser safely
            CloseBrowserSafely();

            // Dispose Unity container
            try
            {
                if (_container != null)
                {
                    _container.Dispose();
                    _container = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "Unity shutdown error: " + ex.Message
                );
            }
        }

        // =========================================================
        // SAFE BROWSER SHUTDOWN
        // =========================================================

        private void CloseBrowserSafely()
        {
            try
            {
                var browser = HTML.SharedBrowser;

                if (browser != null)
                {
                    browser.CloseAsync()
                           .GetAwaiter()
                           .GetResult();
                }
            }
            catch (Exception ex)
            {
                // Best-effort shutdown.
                Debug.WriteLine(
                    "Browser shutdown error: " + ex.Message
                );
            }
        }
    }
}
