function registerFormController($scope, $http, $location) {

    $scope.setRoute = function (route) {
        $location.path(route);
    }
    // create a blank object to hold our form information
    // $scope will allow this to pass between controller and view
    $scope.loginFormData = {};

    // process the form
    $scope.processForm = function () {
        $http({
            method: 'POST',
            url: 'http://localhost:56470/api/Users/register',
            data: $.param($scope.registerFormData),  // pass in data as strings
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }  // set the headers so angular passing info as form data (not request payload)
        })
            .success(function () {
                $location.path('login');
            });

    };

}