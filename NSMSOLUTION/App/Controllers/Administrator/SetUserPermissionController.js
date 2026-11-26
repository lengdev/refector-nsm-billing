app.controller('SetUserPermissionController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Set User Permission";
    $rootScope.header = "Set User Permission";
    $scope.LoadCreatePagePermission = function () {
        Form.LoadHtmlData("Administrator/SetUserPermission", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadCreatePagePermission();
}]);