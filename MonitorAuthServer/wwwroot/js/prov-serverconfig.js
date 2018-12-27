(function () {

    'use strict';

    angular.module('monitorServerConfig', [])
        .provider('ServerConfig', ServerConfigFactory);

    function ServerConfigFactory() {
        var config = {};

        return {
            $get: create,
            load: load
        };

        function create($http) {
            return {
                init: init
            };

            function init() {
                config.id = 0;
            }
        }

        function load() {
            return config;
        }
    }

})();
