'use strict';
app.controller('BillsController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Bills";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Bills", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);