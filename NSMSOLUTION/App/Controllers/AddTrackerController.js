'use strict';
app.controller('AddTrackerController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Tracker";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Tracker", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);