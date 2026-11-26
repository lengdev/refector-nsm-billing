'use strict';
app.controller('AddPositionController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Position";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddPosition", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);