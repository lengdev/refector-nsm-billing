app.controller('CreateUserLoginTmpController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $scope.LoadCreateSystem = function () {
        Form.LoadHtmlData("Administrator/CreateUserLoginTmp", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadCreateSystem();
}]);