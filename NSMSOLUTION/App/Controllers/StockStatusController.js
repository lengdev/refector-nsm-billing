'use strict';
app.controller('StockStatusController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Stock Status";
    $scope.LoadData = function () {
        Form.LoadHtmlData("StockStatus", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);