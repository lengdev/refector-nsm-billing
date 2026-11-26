'use strict';
app.controller('ReportCustomerInfoListController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Customer Info List";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/CustomerInfoList", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);