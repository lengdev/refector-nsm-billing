'use strict';
app.controller('NewCustomerProfileController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "New Customer Profile";
    let ID = $location.search().ID;
    let ISSO = $location.search().ISSO;
    let isRegisterForm = $location.search().isRegisterForm;
    $scope.LoadData = function () {
        Form.LoadHtmlData("NewCustomerProfile", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);