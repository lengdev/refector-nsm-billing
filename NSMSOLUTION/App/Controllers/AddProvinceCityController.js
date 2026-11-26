'use strict';
app.controller('AddProvinceCityController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Province City";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddProvinceCity", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);