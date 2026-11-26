'use strict';
app.controller('UserRoleController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add User Role";
    $scope.LoadData = function () {
        Form.LoadHtmlData("UserRole", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);