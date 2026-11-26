'use strict';
app.controller('AddUomController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Unit of measure";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddUOM", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);