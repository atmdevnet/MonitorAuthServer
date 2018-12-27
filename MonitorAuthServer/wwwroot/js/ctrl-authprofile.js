(function () {

    'use strict';

    angular.module('monitor')
        .controller('authprofile', authprofileController);

    function authprofileController($scope, profile) {
        $scope.profile = profile;
    }

})();
