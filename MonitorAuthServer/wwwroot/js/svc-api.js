(function () {
    'use strict';

    angular.module('monitorApi', [])
        .provider('svcApi', svcApi);

    function svcApi() {
        var provider = {
            $get: create,
            init: init
        };


        var _config = {
            url: {
                licenseAdd: '',
                licenseActive: '',
                licenseExpired: '',
                licenseEdit: '',
                licenseAudit: '',
                licenseRecent: '',
                activity: '',
                versionList: '',
                versionEdit: '',
                allegroSystime: '',
                allegroUser: '',
                allegroUserFieldHeaders: '',
                scopes: ''
            }
        };

        function init(configCallback) {
            if (configCallback && configCallback.constructor === Function) {
                configCallback(_config);
            }
        }

        function create($http) {
            var service = {

                url: {
                    license: {
                        add: _config.url.licenseAdd,
                        edit: _config.url.licenseEdit,
                        active: _config.url.licenseActive,
                        expired: _config.url.licenseExpired,
                        audit: _config.url.licenseAudit,
                        recent: _config.url.licenseRecent,
                        activity: _config.url.activity
                    },
                    version: {
                        list: _config.url.versionList,
                        edit: _config.url.versionEdit
                    }
                },

                addLicense: function (data) {
                    return $http.post(_config.url.licenseAdd, data); //'/api/licenses/add'
                },

                allegroSystemTime: function () {
                    return $http.get(_config.url.allegroSystime); //'/api/allegro/systime'
                },

                allegroUser: function (id, login) {
                    var data = {
                        id: id,
                        login: login
                    };

                    return $http.post(_config.url.allegroUser, data); //'/api/allegro/userinfo'
                },

                allegroUserFieldHeaders: function () {
                    return $http.get(_config.url.allegroUserFieldHeaders); //'/api/allegro/userfieldheaders'
                },

                scopes: function () {
                    return $http.get(_config.url.scopes);
                },

                userLicenseAudit: function (userId) {
                    return $http.get(_config.url.licenseAudit + '/' + userId);
                },

                userActivity: function (userId) {
                    return $http.get(_config.url.activity + '/' + userId);
                },

                removeVersion: function (version) {
                    var data = {
                        action: 'remove',
                        table: '',
                        keyColumn: '',
                        key: 0,
                        value: version,
                        added: [],
                        changed: [],
                        deleted: [],
                        params: undefined
                    };

                    return $http.post(_config.url.versionEdit, data);
                }

            };

            return service;
        }

        return provider;
    }

})();

