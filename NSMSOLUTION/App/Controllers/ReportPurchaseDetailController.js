'use strict';
app.controller('ReportPurchaseDetailController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Purchase Detail";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ReportPurchaseDetail", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);