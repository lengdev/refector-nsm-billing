app.controller('CreateUserController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Create User";
    $rootScope.header = "Create User";
    $scope.LoadCreateUser = function () {
        Form.LoadHtmlData("Administrator/CreateUser", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadCreateUser();
}]);