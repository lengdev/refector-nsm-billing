'use strict';
app.controller('AddDepartmentController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Department";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddDepartment", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);