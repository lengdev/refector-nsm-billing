'use strict';
app.controller('AddWebSlideShowController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Web Slide Show";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddWebSlideShow", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);