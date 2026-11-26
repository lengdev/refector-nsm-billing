'use strict';
app.controller('ReportCustomerIdentificationController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Customer Identification";
    $scope.LoadData = function () {
        var ID = (typeof $location.search().StatusID == "undefined" ? 0 : $location.search().StatusID);
        Form.LoadHtmlData("Report/ReportCustomerIdentification", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
