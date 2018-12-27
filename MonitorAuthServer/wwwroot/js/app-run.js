(function () {

    'use strict';

    angular.module('monitor')
        .run(function (svcAuth) {
            svcAuth.handleAuthentication();
        });

})();
