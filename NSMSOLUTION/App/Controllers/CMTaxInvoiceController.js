'use strict';
app.controller('CMTaxInvoiceController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Customer Invoice List";
    $scope.LoadData = function () {
        var ID = (typeof $location.search().StatusID == "undefined" ? 0 : $location.search().StatusID);
        Form.LoadHtmlData("Report/CMTaxInvoice", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
