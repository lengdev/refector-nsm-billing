'use strict';
app.controller('AdjustmentStockController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Adjustment Stock";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AdjustmentStock", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);