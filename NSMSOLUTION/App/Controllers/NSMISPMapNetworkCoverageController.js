'use strict';
app.controller('NSMISPMapNetworkCoverageController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Network Coverage";
    $scope.LoadData = function () {
        Form.LoadHtmlData("NSMISPMapNetworkCoverage", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);