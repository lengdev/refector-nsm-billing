(function () {
    'use strict';
    app.controller('LoginController', ['$scope', 'LoginService', '$location', '$window',
        'AuthenticationService',
        function ($scope, LoginService, $location, $window, AuthenticationService) {

            $scope.LoginData = {
                UserName: "",
                Password: ""
            };

            $scope.init = function () {
                var accessToken = AuthenticationService.getTokenInfo();
                if (accessToken != "" && accessToken != null)
                    $window.location.href = apiBaseUrl + "Administrator";
            };

            $scope.Login = function () {
                LoginService.Login($scope.LoginData.UserName, $scope.LoginData.Password, function (result) {
                    if (result.Logged) {
                        $window.location.href = apiBaseUrl + "Administrator";
                    }
                });
            }

            $scope.init();
        }]);
})();
////(function () {
////    'use strict';
////    appUser.controller('LoginController', ['$scope', 'LoginService', '$location', '$window', 'authData', function ($scope, LoginService, $location, $window, authData) {

////        $scope.LoginData = {
////            UserName: "",
////            Password: ""
////        };

////        $scope.Login = function () {
////            LoginService.Login($scope.LoginData.UserName, $scope.LoginData.Password, function (result) {
////                if (result.Logged) {                    
////                    $window.location.href = apiBaseUrl;
////                }
////            });
////        }
////        $scope.Logout = function () {
////            LoginService.LogOut();
////        }
////        var accessToken = (typeof sessionStorage.getItem('TokenInfo') == 'undefined') ? "" : sessionStorage.getItem('TokenInfo');
////        if (accessToken != "" && accessToken != null) {
////            $window.location.href = apiBaseUrl;
////        }
////    }]);
////})();