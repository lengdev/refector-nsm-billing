'use strict';
app.controller('CreateQuotationController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Create Quotation";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CreateQuotation", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);