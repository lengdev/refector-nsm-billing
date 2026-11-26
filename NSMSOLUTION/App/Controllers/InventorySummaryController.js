'use strict';
app.controller('InventorySummaryController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Inventory Summary";
    $scope.LoadData = function () {
        Form.LoadHtmlData("InventorySummary", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);