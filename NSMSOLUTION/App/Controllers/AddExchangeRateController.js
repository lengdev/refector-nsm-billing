'use strict';
app.controller('AddExchangeRateController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Exchange Rate";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddExchangeRate", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);