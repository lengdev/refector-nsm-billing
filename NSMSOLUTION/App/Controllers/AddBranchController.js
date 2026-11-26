'use strict';
app.controller('AddBranchController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Branch";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddBranch", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);