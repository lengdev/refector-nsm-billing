'use strict';
app.controller('ReportItemListController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Item List";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/ItemList", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);