'use strict';
app.controller('SupportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Support";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Support", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);