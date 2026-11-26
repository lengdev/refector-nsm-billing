'use strict';
app.controller('AddSplitterNameController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add OLT Name";
    $scope.LoadData = function () {
        Form.LoadHtmlData("OLT/SplitterName", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);