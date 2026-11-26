'use strict';
app.controller('AddUserLoginController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add User Login";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddUserLogin", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);