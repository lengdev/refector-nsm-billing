'use strict';
app.controller('AddDeviceSplitterNameController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Splitter Name";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddDeviceSplitterName", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);