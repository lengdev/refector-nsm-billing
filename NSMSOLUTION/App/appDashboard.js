'use strict';
//var apiBaseUrl = "http://localhost/newinv/";
var apiBaseUrl = "http://localhost:64161/";

var statusNotify = "";
var LoginUrl = apiBaseUrl + "Login";
var app = angular.module("appDashboard", ["ngCookies", "ngRoute", "restangular", "ngTable", "Form", "LocalStorageModule", "chieffancypants.loadingBar", "ngAnimate", "ngSanitize"]);

app.config(function (cfpLoadingBarProvider) {
	cfpLoadingBarProvider.includeSpinner = false;
});

app.config(function ($routeProvider, $httpProvider, RestangularProvider) {


}).run(['$rootScope', '$location', 'AuthData', '$window', 'Form', function ($rootScope, $location, AuthData, $window, Form) {

	//$rootScope.$on("$routeChangeStart", function (event, next, current) {
	var isLoggedIn = false;
	var UserName = "";
	isLoggedIn = AuthData.authenticationData.IsAuthenticated;
	UserName = AuthData.authenticationData.userName;
	if (!isLoggedIn)
		$window.location.href = apiBaseUrl + "Login";
	else
		$rootScope.userName = UserName;
	//});
}]);

(function () {
	'use strict';
	//var accessToken = (typeof sessionStorage.getItem('TokenInfo') == 'undefined') ? "" : sessionStorage.getItem('TokenInfo');
	//if (accessToken == null || accessToken == "") {
	//	$window.location.href = apiBaseUrl + "Login";
	//}
	appUser.controller('LogoutController', ['$scope', 'LoginService', '$window', function ($scope, LoginService, $window) {
		$scope.Logout = function () {
			LoginService.LogOut();
		}
	}]);
})();
(function () {
	'use strict';
	app.controller('GetUserRoleController', ['$scope', 'LoginService', '$window', 'Form', function ($scope, LoginService, $window, Form) {
		Form.LoadData("Home/GetUserRole", function (result) {
			console.log(result.responseText);
		});
	}]);
})();