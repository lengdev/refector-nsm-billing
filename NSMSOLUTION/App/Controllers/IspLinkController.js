'use strict';
app.controller('IspLinkController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "ISP Link";
    $scope.LoadData = function () {
        Form.LoadHtmlData("IspLink", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);