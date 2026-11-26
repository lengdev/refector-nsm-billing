'use strict';
app.controller('AddPaymentCategoryController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Payment Category";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddPaymentCategory", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);