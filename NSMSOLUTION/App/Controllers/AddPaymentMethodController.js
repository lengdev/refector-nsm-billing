'use strict';
app.controller('AddPaymentMethodController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Payment Method";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddPaymentMethod", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);