'use strict';
app.controller('AddStockInController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Stock In";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddStockIn", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);