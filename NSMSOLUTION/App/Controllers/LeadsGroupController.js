'use strict';
app.controller('LeadsGroupController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Leads Group";
    $scope.LoadData = function () {
        Form.LoadHtmlData("LeadsGroup", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);