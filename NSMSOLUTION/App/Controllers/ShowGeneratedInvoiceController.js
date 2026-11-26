'use strict';
app.controller('ShowGeneratedInvoiceController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Show Generated Invoice";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/ShowGeneratedInvoice", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);