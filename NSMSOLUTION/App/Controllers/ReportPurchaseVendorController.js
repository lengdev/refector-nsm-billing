'use strict';
app.controller('ReportPurchaseVendorController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Purchase Vendor";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ReportPurchaseVendor", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);