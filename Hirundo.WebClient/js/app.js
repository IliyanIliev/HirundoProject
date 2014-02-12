var hirundoApp = angular.module('hirundoApp', [
  'ngRoute'
]);

hirundoApp.config(['$routeProvider',
  function ($routeProvider) {
      $routeProvider.
        when('/login', {
            templateUrl: 'partials/login.html',
            controller: 'loginFormController'
        }).
        when('/register', {
            templateUrl: 'partials/register.html'
        }).
        otherwise({
            redirectTo: '/login'
        });
  }]);