app.controller('AdministratorController', ['$rootScope', '$scope', 'LoginService', 'Form',
    function ($rootScope, $scope, LoginService, Form) {
        $rootScope.title = "Administrator";
        Form.LoadHtmlData("Administrator/Dashboard", function (result) {
            $scope.HtmlData = result;
        });
    }]);