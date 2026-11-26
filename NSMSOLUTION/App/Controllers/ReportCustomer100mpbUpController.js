'use strict';
app.controller('ReportCustomer100mpbUpController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Customer 100 mpbUp";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ReportCustomer100mpbUp", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
//ReportCustomer100mbpUp
//ReportCustomer100mpbUp