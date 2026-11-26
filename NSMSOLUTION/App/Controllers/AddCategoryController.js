app.controller('AddCategoryController', ['$scope', 'Form', function ($scope, Form) {
    'use strict';
    $scope.HtmlData = "";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddCategory", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);