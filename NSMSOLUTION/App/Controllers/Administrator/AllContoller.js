app.controller('AdministratorController', ['$rootScope', '$scope', 'LoginService', 'Form',
    function ($rootScope, $scope, LoginService, Form) {
        $rootScope.title = "Administrator";
        Form.LoadHtmlData("Administrator/Dashboard", function (result) {
            $scope.HtmlData = result;
        });
    }]);
app.controller('CreatePageCategoryController', ['$rootScope', '$scope', 'LoginService', 'Form',
    function ($rootScope, $scope, LoginService, Form) {
        $rootScope.title = "Create Page Category";
        Form.LoadHtmlData("Administrator/CreatePageCategory", function (result) {
            $scope.HtmlData = result;
        });
    }]);
app.controller('PagenotfoundController', ['$rootScope', '$scope', 'Form',
    function ($rootScope, $scope, Form) {
        $rootScope.title = "Page not found";
    }]);
app.controller('CreatePagePermissionController', ['$rootScope', '$scope', 'LoginService', 'Form',
    function ($rootScope, $scope, LoginService, Form) {
        $rootScope.title = "Create Page Permission";
        Form.LoadHtmlData("Administrator/CreatePagePermission", function (result) {
            $scope.HtmlData = result;
        });
    }]);
app.controller('AddPageToSystemController', ['$rootScope', '$scope', 'LoginService', 'Form',
    function ($rootScope, $scope, LoginService, Form) {
        $rootScope.title = "AddPage To System";
        Form.LoadHtmlData("Administrator/AddPageToSystem", function (result) {
            $scope.HtmlData = result;
        });
    }]);

app.controller('CreateWarehouseController', ['$rootScope', '$scope', 'LoginService', 'Form',
    function ($rootScope, $scope, LoginService, Form) {
        $rootScope.title = "Create Warehouse";
        Form.LoadHtmlData("Administrator/CreateWarehouse", function (result) {
            $scope.HtmlData = result;
        });
    }]);

app.controller('CreateSystemController', ['$rootScope', '$scope', 'LoginService', 'Form',
    function ($rootScope, $scope, LoginService, Form) {
        $rootScope.title = "Create System";
        Form.LoadHtmlData("Administrator/CreateSystem", function (result) {
            $scope.HtmlData = result;
        });
    }]);
app.controller('CreateUserController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Create User";
    Form.LoadHtmlData("Administrator/CreateUser", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('SetUserPermissionController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Set User Permission";
    Form.LoadHtmlData("Administrator/SetUserPermission", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('UserRoleController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "User Role";
    Form.LoadHtmlData("Administrator/UserRole", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('SlideShowController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Slide Show";
    Form.LoadHtmlData("Administrator/SlideShow", function (result) {
        $scope.HtmlData = result;
    });
}]);

app.controller('CreateTableController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Create Table";
    Form.LoadHtmlData("Administrator/Table", function (result) {
        $scope.HtmlData = result;
    });
}]);

app.controller('ShowUserByRoleController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Show User By Role";
    Form.LoadHtmlData("Administrator/ShowUserByRole", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('AddDomainNameController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Add Domain Name";
    Form.LoadHtmlData("Administrator/AddDomainName", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('SetAccessDomainNameController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Set Access Domain Name";
    Form.LoadHtmlData("Administrator/SetAccessDomainName", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('AddTemplateNameController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Add Template Name";
    Form.LoadHtmlData("Administrator/AddTemplateName", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('AddUserTemplateController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Add User Template";
    Form.LoadHtmlData("Administrator/AddUserTemplate", function (result) {
        $scope.HtmlData = result;
    });
}]);