'use strict';
app.controller('ShowCurrentBlockNumberController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Show Current Block Number";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ShowCurrentBlockNumber", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);