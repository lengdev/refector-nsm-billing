app.controller('UserRoleController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "User Role";
    $rootScope.header = "Create User Role";
    $scope.LoadCreateSystem = function () {
        Form.LoadHtmlData("Administrator/UserRole", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadCreateSystem();
}]);