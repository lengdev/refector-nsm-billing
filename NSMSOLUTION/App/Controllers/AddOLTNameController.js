'use strict';
app.controller('AddOLTNameController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add OLT Name";
    $scope.LoadData = function () {
        Form.LoadHtmlData("OLT/OLTName", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);