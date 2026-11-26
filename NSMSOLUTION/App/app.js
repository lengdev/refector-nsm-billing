"use strict";

window.base_url = location.protocol + "//" + location.host + "/";
window.apiBaseUrl = window.base_url;

var ServerRoot = window.base_url;

var app = angular.module("appInventory", [
  "ngCookies",
  "ngStorage",
  "ngRoute",
  "restangular",
  "ngTable",
  "Form",
  "LocalStorageModule",
  "chieffancypants.loadingBar",
  "ngAnimate",
  "ngSanitize",
  "angular.filter",
  "ngHtmlCompile",
  "angular-web-notification",
]);

app
  .config([
    "$httpProvider",
    "RestangularProvider",
    "cfpLoadingBarProvider",
    function ($httpProvider, RestangularProvider, cfpLoadingBarProvider) {
      RestangularProvider.setBaseUrl(window.apiBaseUrl);
      cfpLoadingBarProvider.includeSpinner = false;
      $httpProvider.defaults.withCredentials = true;
    },
  ])
  .run([
    "$interval",
    "$injector",
    function ($interval, $injector) {
      var CHECK_INTERVAL_MS = 60 * 1000;
      var REFRESH_BEFORE_EXPIRY_MS = 1 * 60 * 1000; // 1min before expiry
      var isRefreshing = false;
      function checkAndRefreshToken() {
        var AuthenticationService = $injector.get("AuthenticationService");
        var tokenInfo = AuthenticationService.getTokenInfo();
        if (!tokenInfo || !tokenInfo.accessToken || !tokenInfo.TokenExpires)
          return;
        var expiresAt = new Date(tokenInfo.TokenExpires).getTime();
        var msUntilExpiry = expiresAt - Date.now();
        if (msUntilExpiry <= REFRESH_BEFORE_EXPIRY_MS && !isRefreshing) {
          isRefreshing = true;
          var LoginService = $injector.get("LoginService");
          LoginService.refreshToken().then(
            function () {
              isRefreshing = false;
            },
            function () {
              isRefreshing = false;
            },
          );
        }
      }
      $interval(checkAndRefreshToken, CHECK_INTERVAL_MS);
      checkAndRefreshToken();
    },
  ]);

app.controller("SysLogoController", [
  "$scope",
  "$location",
  function ($scope, $location) {
    var port = $location.port();
    var host = $location.host();
    var protocol = $location.protocol();

    $scope.absUrl = protocol + "://" + host + (port ? ":" + port : "") + "/";

    $scope.SysWLogo = localStorage.getItem("SysWLogo");
    $scope.$watch(
      function () {
        return $location.path();
      },
      function (newPath, oldPath) {
        var defaultPage = localStorage.getItem("DefaultDashboardPage");
        var isRoot = newPath === "/" || newPath === "" || newPath === "!";

        if (isRoot && defaultPage) {
          var cleanTarget = oldPath
            ? oldPath.replace("/", "")
            : defaultPage.replace(/^#?\/?/, "");
          var finalPath = "/" + cleanTarget;

          if (newPath !== finalPath) {
            $location.path(finalPath).replace();
          }
        }
      },
    );
  },
]);

app.directive("errSrc", function () {
  return {
    link: function (scope, element, attrs) {
      element.on("error", function () {
        if (attrs.src !== attrs.errSrc) {
          attrs.$set("src", attrs.errSrc);
        }
      });
    },
  };
});
