'use strict';
app.controller('CustomerSaleOrderProfileController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Customer Sale Order Profile";
    let ID = $location.search().ID;
    let ISSO = $location.search().ISSO;
    let isRegisterForm = $location.search().isRegisterForm;
    $scope.LoadData = function () {
        //+ "&isRegisterForm=" + isRegisterForm
        Form.LoadHtmlData("CustomerSaleOrderProfile?ID=" + ID + "&ISSO=" + ISSO, function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);