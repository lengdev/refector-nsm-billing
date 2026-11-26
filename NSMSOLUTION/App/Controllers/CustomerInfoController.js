'use strict';
app.controller('CustomerInfoController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Info";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerProfile/CustomerInfo", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);