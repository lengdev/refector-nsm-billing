'use strict';
app.controller('BalanceSheetController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Balance sheet";
    $scope.LoadData = function () {
        Form.LoadHtmlData("BalanceSheet", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);