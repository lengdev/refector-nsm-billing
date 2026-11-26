'use strict';
app.controller('AddAccountController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Account";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddAccount", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);