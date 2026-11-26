'use strict';
app.controller('AddCustomerController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Customer";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddCustomer", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);