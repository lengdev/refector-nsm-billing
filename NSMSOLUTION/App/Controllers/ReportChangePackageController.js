'use strict';
app.controller('ReportChangePackageController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Change Package";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ReportChangePackage", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
