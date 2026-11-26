'use strict';
app.controller('RadiusUserListsVlanController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Radius User Lists Vlan";
    $scope.LoadData = function () {
        Form.LoadHtmlData("RADIUS/RadiusUserListsVlan", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);