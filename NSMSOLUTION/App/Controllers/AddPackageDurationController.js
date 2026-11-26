'use strict';
app.controller('AddPackageDurationController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Create Department";
    $rootScope.header = "Create Department";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddPackageDuration", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);