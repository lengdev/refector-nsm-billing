'use strict';
app.controller('AddWorkingTimeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Working Time";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddWorkingTime", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);