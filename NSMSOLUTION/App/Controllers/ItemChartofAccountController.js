'use strict';
app.controller('ItemChartofAccountController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Item Chart of Account";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ItemChartofAccount", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
