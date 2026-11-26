'use strict';
app.controller('ReportCustomer100MbpsUpController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Customer 100 Mbps Up";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ReportCustomer100MbpsUp", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
