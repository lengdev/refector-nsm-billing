'use strict';
app.controller('AddStockNameController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Stock Name";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddStockName", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);