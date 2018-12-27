(function () {

    'use strict';

    angular.module('monitor')
        .directive('login', login);

    function login() {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/views/directive/login.html',
            controller: loginController
        };
    }

    function loginController($scope, $location, svcAuth) {
        $scope.login = function () {
            svcAuth.login();
        }

        $scope.logout = function () {
            svcAuth.logout();
        }

        $scope.authenticated = function () {
            return svcAuth.isAuthenticated();
        }

        $scope.profile = function () {
            $location.url('/authprofile');
        }

        $scope.renew = function () {
            svcAuth.renew();
        }

        $scope.canRenew = function () {
            return $scope.authenticated() && $scope.expiryNotified == true;
        }

        $scope.expiryNotified = false;
        $scope.sessionTimeout = '';


        svcAuth.registerNotifySessionExpiryCallback(notifySessionExpiry);


        function notifySessionExpiry(expires) {
            if (expires && expires.constructor === Date) {
                var leftms = expires.getTime() - new Date().getTime();
                if (leftms > 0) {
                    var lefts = leftms / 1000;
                    var leftmin = Math.floor(lefts / 60);
                    var leftsec = Math.floor(lefts % 60);

                    $scope.expiryNotified = true;
                    $scope.sessionTimeout = 'Sesja wygasa za ' + leftmin + ' min. ' + leftsec + ' sek.';
                }
                else {
                    cancelNotifySessionExpiry();
                }
            }
        }

        function cancelNotifySessionExpiry() {
            $scope.expiryNotified = false;
        }
    }

})();
