'use strict';
app.controller('AccountTypeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Account Type";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AccountType", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);