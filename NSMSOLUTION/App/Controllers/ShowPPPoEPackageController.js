'use strict';
app.controller('ShowPPPoEPackageController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Show PPPoE Package";
    $scope.LoadData = function () {
        Form.LoadHtmlData("RADIUS/ShowPPPoEPackage", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);