'use strict';
app.controller('CustomerPaymentController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Payment";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Customer/Payment", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);