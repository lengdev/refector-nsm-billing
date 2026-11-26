'use strict';
app.controller('ShowPPPoELimitationController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Show PPPoE Limitation";
    $scope.LoadData = function () {
        Form.LoadHtmlData("RADIUS/ShowPPPoELimitation", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);