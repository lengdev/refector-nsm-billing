'use strict';
app.controller('SummaryByVoucherController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Summary By Voucher";
    $scope.LoadData = function () {
        Form.LoadHtmlData("SummaryByVoucher", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);