'use strict';
app.controller('SetDefaultOnAddItemFormController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Set Default On Add Item Form";
    $scope.LoadData = function () {
        Form.LoadHtmlData("SetDefaultOnAddItemForm", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);