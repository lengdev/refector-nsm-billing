app.controller('AddPageToSystemController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "AddPage To System";
    $rootScope.header = "AddPage To System";
    $scope.LoadCreateSystem = function () {
        Form.LoadHtmlData("Administrator/AddPageToSystem", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadCreateSystem();
}]);