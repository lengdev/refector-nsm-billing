'use strict';
app.controller('SupportUserRoleController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add User Role";
    $scope.LoadData = function () {
        Form.LoadHtmlData("SupportUserRole", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);