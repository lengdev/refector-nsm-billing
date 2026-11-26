'use strict';
app.controller('AddCurrencyController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Currency";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddCurrency", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);