app.directive('accessLevel', ['Auth', function (Auth) {
    var accessLevels = routingConfig.accessLevels;
    return {
        restrict: 'A',
        link: function ($scope, element, attrs) {
            var prevDisp = element.css('display')
                , userRole = ""
                , accessLevel = "";

            $scope.user = Auth.user;
            $scope.$watch('user', function (user) {
                if (user.role)
                    userRole = user.role;
                updateCSS();
            }, true);

            attrs.$observe('accessLevel', function (al) {
                var aLevel = accessLevels[al];
                if (al) accessLevel = aLevel;
                updateCSS();
            });

            function updateCSS() {
                if (!Auth.authorize(accessLevel, userRole))
                    element.css('display', 'none');
                else
                    element.css('display', prevDisp);
            }
        }
    };
}]);

app.directive('activeNav', ['$location', function ($location) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            var nestedA = element[0];//.find('a');
            var path = nestedA.href;
            scope.location = $location;
            scope.$watch('location.absUrl()', function (newPath) {
                if (path === newPath) {
                    element.addClass('active');
                } else {
                    element.removeClass('active');
                }
            });
        }
    };
}]);