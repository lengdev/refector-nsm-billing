'use strict';
app.controller('AddActivateCustomerController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Activate Customer";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ActivateCustomer", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);