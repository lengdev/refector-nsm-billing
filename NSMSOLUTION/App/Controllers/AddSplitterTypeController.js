'use strict';
app.controller('AddSplitterTypeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Splitter Type";
    $scope.LoadData = function () {
        Form.LoadHtmlData("OLT/SplitterType", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);