(function () {
    "use strict";
    app.controller("LoginController", [
        "$scope",
        "LoginService",
        "$location",
        "$window",
        "AuthenticationService",
        function (
            $scope,
            LoginService,
            $location,
            $window,
            AuthenticationService,
        ) {
            $scope.isLoggingIn = false;
            $scope.LoginData = {
                UserName: "",
                Password: "",
            };
            $scope.init = function () {
                var accessToken = AuthenticationService.getTokenInfo();
                if (accessToken != "" && accessToken != null)
                    $window.location.href = apiBaseUrl + "Home/Dashboard";
            };

            $scope.Login = function () {
                $scope.isLoggingIn = true;
                LoginService.Login(
                    $scope.LoginData.UserName,
                    $scope.LoginData.Password,
                    function (result) {
                        if (result.Logged) {
                            $window.location.href =
                                apiBaseUrl + "Home/Dashboard";
                        } else {
                            $scope.isLoggingIn = false;
                            $("#loginErrorText").text(
                                result.Message ||
                                    "Invalid username or password.",
                            );
                            $("#loginErrorMsg").addClass("is-visible");
                        }
                    },
                );
            };
            $scope.init();
        },
    ]);
})();
