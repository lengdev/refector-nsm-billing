'use strict';
app.controller('ReportCustomerNearlyExpiredController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Customer Nearly Expired";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/CustomerNearlyExpired", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);