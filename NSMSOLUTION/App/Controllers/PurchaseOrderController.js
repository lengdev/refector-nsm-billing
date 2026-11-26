'use strict';
app.controller('PurchaseOrderController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Purchase Order";
    $scope.LoadData = function () {
        Form.LoadHtmlData("PurchaseOrder", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);