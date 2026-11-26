var tmp = '<ol class="breadcrumb" style="background-color:white; padding:6px 0 0 0">';
tmp += '<li data-ng-repeat="state in breadcrumbs" class="breadcrumb-item">';
tmp += '<a href="{{state.path }}" data-ng-if="!$last">{{state.name}}</a>';
tmp += '<span data-ng-if="$last">{{state.name}}</span>';
tmp += '</li>';
tmp += '</ol>';

var breadcrumbs = function ($location) {
    return {
        restrict: 'E',
        template: tmp,
        replace: true,
        compile: function (tElement, tAttrs) {
            return function ($scope, $elem, $attr) {
                $scope.$on('$routeChangeStart', function (event, next) {
                    var pathElements = $location.path().split('/'), result = [], i;                    
                    var breadcrumbPath = function (index) {
                        return '/' + (pathElements.slice(0, index + 1)).join('/');
                    };
                    pathElements.shift();
                    for (i = 0; i < pathElements.length; i++) {
                        if (i == 0) {
                            result.push({ name: 'Home', path: 'Home/Dashboard' });
                            result.push({ name: 'Dashboard', path: "#!/" + getStorage("DefaultDashboardPage") });
                        }
                        else {
                            result.push({ name: pathElements[i], path: breadcrumbPath(i) });
                        }
                    }
                    if (pathElements.length == 1) {
                        var controller = (typeof next.params.controller == 'undefined') ? "" : next.params.controller;
                        if (controller != "") {
                            result.push({ name: controller, path: '' });
                        }
                    }
                    $scope.breadcrumbs = result;
                });
            }
        }
    };
};

app.directive('breadcrumbs', ['$location', breadcrumbs]);