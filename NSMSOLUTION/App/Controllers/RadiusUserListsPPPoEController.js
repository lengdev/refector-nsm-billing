'use strict';
app.controller('RadiusUserListsPPPoEController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Radius User Lists PPPoE";
    $scope.LoadData = function () {
        Form.LoadHtmlData("RADIUS/RadiusUserListsPPPoE", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('RouterLogController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Router Log";
    Form.LoadHtmlData("RouterLog", function (result) {
        $scope.HtmlData = result;
    });
}]);