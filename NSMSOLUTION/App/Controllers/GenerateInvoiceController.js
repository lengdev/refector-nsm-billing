'use strict';
app.controller('GenerateInvoiceController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Generate Auto Invoices & Bills";
    $scope.LoadData = function () {
        Form.LoadHtmlData("GenerateInvoice", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);