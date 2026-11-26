'use strict';
app.controller('ReportCustomerServiceController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Customer Services";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/CustomerService", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);