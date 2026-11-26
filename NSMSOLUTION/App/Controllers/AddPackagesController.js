'use strict';
app.controller('AddPackagesController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Packages";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddPackages", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);