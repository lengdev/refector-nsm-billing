app.controller('SetPagePermissionController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Set Page Permission";
    $rootScope.header = "Set Page Permission";
    $scope.LoadCreatePagePermission = function () {
        Form.LoadHtmlData("Administrator/CreatePagePermission", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadCreatePagePermission();
}]);