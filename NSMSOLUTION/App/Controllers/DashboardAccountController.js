'use strict';
app.controller('DashboardAccountController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "dashboard Accounting ";
    $scope.LoadData = function () {
        Form.LoadHtmlData("DashboardAccount", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);