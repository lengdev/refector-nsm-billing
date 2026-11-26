'use strict';
app.controller('AddTaxController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Tax";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddTax", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);