'use strict';
app.controller('LeadsController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Leads";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Leads", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);