'use strict';
app.controller('VendorUnpaidController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Vendor Unpaid";
    $scope.LoadData = function () {
        Form.LoadHtmlData("VendorUnpaid/Unpaid", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);