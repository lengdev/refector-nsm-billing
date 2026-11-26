'use strict';
app.controller('ReportCustomerTerminateController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Customer Terminate";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ReportCustomerTerminate", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);