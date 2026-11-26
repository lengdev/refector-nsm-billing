'use strict';
app.controller('AddEmployeesController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Employee";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddEmployees", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);