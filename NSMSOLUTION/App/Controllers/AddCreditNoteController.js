'use strict';
app.controller('AddCreditNoteController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add CreditNote";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerProfile/LoadInvoiceForCreditNotes?ID=0", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);