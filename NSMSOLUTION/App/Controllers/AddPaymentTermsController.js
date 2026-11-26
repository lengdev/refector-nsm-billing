'use strict';
app.controller('AddPaymentTermsController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Create Company";
    $rootScope.header = "Create Company";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddPaymentTerms", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);