'use strict';
app.controller('AddOrderTypeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Order Type";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddOrderType", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);