'use strict';
app.controller('ChartOfAccountController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Chart Of Account";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ChartOfAccount", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);