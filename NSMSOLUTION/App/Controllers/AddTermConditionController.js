'use strict';
app.controller('AddTermConditionController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Create Department";
    $rootScope.header = "Create Department";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddTermCondition", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);