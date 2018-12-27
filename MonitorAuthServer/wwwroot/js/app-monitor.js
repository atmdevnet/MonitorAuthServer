(function () {

    'use strict';

    angular.module('monitor', ['ngRoute', 'auth0.auth0', 'angular-jwt', 'monitorAuth', 'monitorLibrary', 'monitorApi'])
        .config(routeConfig);


    function routeConfig($routeProvider, $locationProvider, $httpProvider, svcApiProvider, svcAuthProvider, angularAuth0Provider, jwtOptionsProvider) {
        $routeProvider
            .when('/', {
                templateUrl: '/views/monitor.html',
                controller: 'monitor',
                resolve: {
                    userFieldHeaders: function (svcApi) {
                        return svcApi.allegroUserFieldHeaders();
                    },
                    scopes: function (svcApi) {
                        return svcApi.scopes();
                    }
                }
            })
            .when('/authprofile', {
                templateUrl: '/views/authprofile.html',
                controller: 'authprofile',
                resolve: {
                    profile: function (svcAuth) {
                        return svcAuth.authProfile();
                    }
                }
            })
            .when('/login', {
                templateUrl: '/views/login.html',
                controller: 'login'
            })
            .otherwise({
                templateUrl: '/views/404.html'
            });
            //.when('/authcallback', {
            //    templateUrl: '/views/authcallback.html',
            //    controller: 'authcallback'
            //})


        svcAuthProvider.init(function (cfg) {
            cfg.url.error = '/home/error';
            cfg.url.authCallback = '/login';
            cfg.url.authSilentCallback = '/silent';
            cfg.url.profile = '/authprofile';
            cfg.renew.intervalWidthMin = 5;
            cfg.renew.intervalDivCount = 5;
        });

        svcApiProvider.init(function (cfg) {
            cfg.url.licenseAdd = '/api/licenses/add';
            cfg.url.licenseEdit = '/api/licenses/edit';
            cfg.url.licenseActive = '/api/licenses/active';
            cfg.url.licenseExpired = '/api/licenses/expired';
            cfg.url.licenseAudit = '/api/licenses/audit';
            cfg.url.licenseRecent = '/api/licenses/recent';
            cfg.url.versionList = '/api/version/list';
            cfg.url.versionEdit = '/api/version/edit';
            cfg.url.activity = '/api/licenses/activity',
            cfg.url.allegroSystime = '/api/allegro/systime';
            cfg.url.allegroUser = '/api/allegro/userinfo';
            cfg.url.allegroUserFieldHeaders = '/api/allegro/userfieldheaders';
            cfg.url.scopes = '/api/licenses/scopes';
        });

        angularAuth0Provider.init({
            clientID: '',
            domain: 'atmdev.eu.auth0.com',
            responseType: 'token id_token',
            audience: 'https://atmdev.eu.auth0.com/userinfo', 
            redirectUri: 'http://localhost:54456/login',
            scope: 'openid profile'
        });


        jwtOptionsProvider.config({
            tokenGetter: function () {
                return localStorage.getItem('access_token');
            },
            whiteListedDomains: ['localhost']
        });

        $httpProvider.interceptors.push('jwtInterceptor');


        //$locationProvider.hashPrefix('?');
        $locationProvider.html5Mode(true);
    }

})();