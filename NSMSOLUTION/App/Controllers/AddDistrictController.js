'use strict';
app.controller('AddDistrictController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add District";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddDistrict", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);