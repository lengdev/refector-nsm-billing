'use strict';
app.controller('CustomerInvoiceListController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Customer Invoice List";
    $scope.LoadData = function () {
        var ID = (typeof $location.search().StatusID == "undefined" ? 0 : $location.search().StatusID);
        Form.LoadHtmlData("Report/CustomerInvoiceList?StatusID=" + ID, function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
