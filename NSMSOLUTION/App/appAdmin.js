"use strict";
app.controller("LogoutController", [
    "$scope",
    "LoginService",
    "$window",
    function ($scope, LoginService, $window) {
        $scope.Logout = function () {
            LoginService.LogOut("Administrator");
        };
    },
]);

var statusNotify = "";
var LoginUrl = apiBaseUrl + "Administrator/Login";

app.config(function ($routeProvider, $httpProvider, RestangularProvider) {
    $routeProvider
        .when("/", {
            templateUrl: function () {
                return apiBaseUrl + "App/Views/Administrator/index.html";
            },
        })
        .when("/:controller", {
            templateUrl: function (attr) {
                return (
                    apiBaseUrl + "App/Views/" + attr.controller + "/index.html"
                );
            },
        })
        .when("/:controller/:action", {
            templateUrl: function (attr) {
                var action =
                    typeof attr.action == "undefined"
                        ? "index.html"
                        : attr.action + ".html";
                return (
                    apiBaseUrl + "App/Views/" + attr.controller + "/" + action
                );
            },
        })
        .when("/:controller/:action:id", {
            templateUrl: function (attr) {
                var action =
                    typeof attr.action == "undefined"
                        ? "index.html"
                        : attr.action + ".html";
                return (
                    apiBaseUrl +
                    "App/Views/" + // was missing — same bug flagged earlier
                    attr.controller +
                    "/" +
                    action +
                    "?id=" +
                    attr.id
                );
            },
        })
        .otherwise({
            templateUrl: function () {
                return (
                    apiBaseUrl +
                    "App/Views/Administrator/Pagenotfound/index.html"
                );
            },
        });

    $httpProvider.interceptors.push(
        function (
            $q,
            $rootScope,
            $window,
            $location,
            AuthenticationService,
            AuthData,
            $injector,
        ) {
            return {
                request: function (config) {
                    var accessToken = AuthenticationService.getTokenInfo();
                    accessToken =
                        accessToken === "" ? "" : accessToken.accessToken;

                    if (!accessToken) {
                        $window.location.href =
                            apiBaseUrl + "Administrator/Login";
                    } else {
                        // Read fresh from storage every request — this is
                        // what appHome does and appAdmin was skipping.
                        let TokenInfo = getStorage("TokenInfo");
                        let sysInfo = JSON.parse(
                            JSON.parse(TokenInfo).SysAndWH,
                        )[0];
                        let userinfo = JSON.parse(TokenInfo);

                        access_Token = userinfo.accessToken;
                        User_ID = sysInfo.UserID;
                        System_ID = sysInfo.SystemID;
                        Warehouse_ID = sysInfo.WarehouseID;
                        Domain_ID = sysInfo.DomainID;
                        User_Name = userinfo.userName;
                        User_Full_Name = userinfo.UserFullName;
                        UserProfilePicture = userinfo.UserProfilePicture;

                        headerPara = {
                            UserID: User_ID,
                            UserName: User_Name,
                            SystemID: System_ID,
                            WarehouseID: Warehouse_ID,
                            UserFullName: User_Full_Name,
                            DomainID: Domain_ID,
                            UserPosition: User_Poistion,
                            SystemLogo: getStorage("SysWLogo"),
                        };

                        setStorage("WarehouseID", Warehouse_ID);
                        setStorage("SystemID", System_ID);
                        setStorage("DomainID", Domain_ID); // was the undeclared var `DomainID`

                        $rootScope.UserName = getStorage("User_Name");
                        config.headers["x-custom-header"] =
                            JSON.stringify(headerPara);
                        config.headers["Authorization"] =
                            "Bearer " + userinfo.accessToken;
                    }
                    return config;
                },
                requestError: function (rejection) {
                    return $q.reject(rejection);
                },
                response: function (response) {
                    return response;
                },
                responseError: function (rejection) {
                    var isTokenEndpoint =
                        rejection.config &&
                        rejection.config.url &&
                        rejection.config.url.indexOf("token") !== -1;

                    if (
                        rejection.status === 401 &&
                        !isTokenEndpoint &&
                        !rejection.config.__isRetry
                    ) {
                        var $http = $injector.get("$http");
                        var LoginService = $injector.get("LoginService");

                        return LoginService.refreshToken().then(
                            function (userInfo) {
                                rejection.config.__isRetry = true;
                                rejection.config.headers["Authorization"] =
                                    "Bearer " + userInfo.accessToken;
                                return $http(rejection.config);
                            },
                            function () {
                                $window.location.href =
                                    apiBaseUrl + "Administrator/Login";
                                return $q.reject(rejection);
                            },
                        );
                    }

                    if (rejection.status === 404) {
                        $location.path("/Pagenotfound");
                    }
                    return $q.reject(rejection);
                },
            };
        },
    );
}).run([
    "$rootScope",
    "$location",
    "AuthData",
    "$window",
    "Form",
    "cfpLoadingBar",
    function ($rootScope, $location, AuthData, $window, Form, cfpLoadingBar) {
        $rootScope.$on("$routeChangeStart", function () {
            cfpLoadingBar.start();
        });
        $rootScope.$on("$routeChangeSuccess", function (event, current) {
            $rootScope.title = current.$$route.title;
            cfpLoadingBar.complete();
        });
        $rootScope.$on(
            "$routeChangeError",
            function (event, current, previous, rejection) {
                cfpLoadingBar.complete();
                console.error("Route/template load failed:", rejection);
                $location.path("/Pagenotfound");
            },
        );
    },
]);

app.controller("MenuController", [
    "$scope",
    "$compile",
    "AuthenticationService",
    function ($scope, $compile, AuthenticationService) {
        $scope.Menu = [];

        // Load menu from token
        var token = AuthenticationService.getTokenInfo();
        if (token && token.Menu) {
            try {
                $scope.Menu = JSON.parse(token.Menu);
            } catch (e) {
                console.error("Invalid Menu JSON");
            }
        }

        // Menu HTML template
        var template = `<ul class="nav nav-pills nav-sidebar flex-column" 
                data-widget="treeview" role="menu" data-accordion="false">

				<li class="nav-item has-treeview menu-open">
					<a href="javascript:void(0);" class="nav-link active">
						<i class="nav-icon fas fa-tachometer-alt"></i>
						<p>
							MENU
							<svg xmlns="http://www.w3.org/2000/svg" aria-hidden="true" 
								class="right iconify iconify--ph" width="1em" height="1em" 
								viewBox="0 0 256 256">
								<path fill="currentColor"
									d="m181.66 133.66l-80 80a8 8 0 0 1-11.32-11.32L164.69 128L90.34 53.66a8 8 0 0 1 11.32-11.32l80 80a8 8 0 0 1 0 11.32">
								</path>
							</svg>
						</p>
					</a>

					<ul class="nav nav-treeview">
						<li class="nav-item" ng-repeat="value in Menu | filter:{MenuID:1}">
							<a href="{{ value.PageLink }}" class="nav-link" active-nav="active">
								<i class="{{ value.NAVICON || 'ti ti-inner-shadow-bottom-left' }} nav-icon"></i>
								<p class="text-nowrap">{{ value.PermissionPageCode }}</p>
							</a>
						</li>
					</ul>
				</li>
			</ul>`;

        // Compile + append to #admin-menu
        var element = angular.element(document.querySelector("#admin-menu"));
        element.html($compile(template)($scope));

        // Re-init treeview after DOM update
        setTimeout(function () {
            $('[data-widget="treeview"]').Treeview("init");
        }, 50);
    },
]);
