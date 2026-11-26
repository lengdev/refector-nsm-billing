'use strict';
app.controller('AddIndustryTypeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Business";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddIndustryType", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);