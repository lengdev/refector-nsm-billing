'use strict';
app.controller('AddStockReasonController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Stock Reason";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddStockReason", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);