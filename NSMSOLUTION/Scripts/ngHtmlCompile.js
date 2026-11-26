angular.module('ngHtmlCompile', []).
	//   directive('ngHtmlCompile', function($compile) {
	//return {
	//    restrict: 'A',
	//    link: function(scope, element, attrs) {
	//	scope.$watch(attrs.ngHtmlCompile, function(newValue, oldValue) {
	//	    element.html(newValue);
	//	    $compile(element.contents())(scope);
	//	});
	//    }
	//}
	directive('compile', function ($compile, $timeout) {
		return {
			restrict: 'A',
			link: function (scope, element, attrs) {
				$timeout(function () {
					$compile(element.contents())(scope);
				});
			}
		}
	});
