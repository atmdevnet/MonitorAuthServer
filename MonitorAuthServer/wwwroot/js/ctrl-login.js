(function () {

    'use strict';

    angular.module('monitor')
        .controller('login', loginController);

    function loginController($scope, $location, $timeout, svcAuth) {
        $timeout(function () {
            if (svcAuth.isAuthenticated()) {
                $location.url(svcAuth.url.profile);
            }
            else {
                svcAuth.login();
            }
        }, 1000);
    }

})();
