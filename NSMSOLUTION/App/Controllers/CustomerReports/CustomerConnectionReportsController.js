'use strict';
app.controller('CustomerConnectionReportsController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Connection Reports";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/Connection", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('ConnectionByGroupController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Connection By Group Reports";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/ConnectionByGroup", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);