'use strict';
app.controller('ReportCustomerConnectivityListController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Customer Connectivity List";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/CustomerConnectivityList", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);