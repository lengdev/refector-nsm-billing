app.controller('CreateSystemController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Create System";
    $rootScope.header = "Create System";
    $scope.LoadCreateSystem = function () {
        Form.LoadHtmlData("Administrator/CreateSystem", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadCreateSystem();
}]);