app.controller('CreatePageCategoryController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Create Page Category";
    $rootScope.header = "Create Page Category";
    $scope.LoadCreatePageCategory = function () {
        Form.LoadHtmlData("Administrator/CreatePageCategory", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadCreatePageCategory();
}]);