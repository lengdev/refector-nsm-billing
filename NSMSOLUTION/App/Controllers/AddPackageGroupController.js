'use strict';
app.controller('AddPackageGroupController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Package Group";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddPackageGroup", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);