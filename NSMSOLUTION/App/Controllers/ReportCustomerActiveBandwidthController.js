'use strict';
app.controller('ReportCustomerActiveBandwidthController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Customer Active Bandwidth";
    $scope.LoadData = function () {
        var ID = (typeof $location.search().StatusID == "undefined" ? 0 : $location.search().StatusID);
        Form.LoadHtmlData("Report/ReportCustomerActiveBandwidth", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
