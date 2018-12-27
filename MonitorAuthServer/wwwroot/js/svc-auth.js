(function () {

    'use strict';

    angular.module('monitorAuth', ['monitorLibrary'])
        .provider('svcAuth', svcAuth);

    function svcAuth() {
        var provider = {
            $get: create,
            init: init
        };


        var _config = {
            url: {
                error: '',
                authCallback: '',
                authSilentCallback: '',
                profile: ''
            },
            renew: {
                intervalWidthMin: 5,
                intervalDivCount: 5
            }
        };

        function init(configCallback) {
            if (configCallback && configCallback.constructor === Function) {
                configCallback(_config);
            }
        }

        function create(angularAuth0, $timeout, $interval, $window, $location, $q, jwtHelper, svcLib) {
            var service = {

                url: {
                    profile: _config.url.profile
                },

                handleAuthentication: function () {
                    angularAuth0.parseHash(function (error, authResult) {
                        if (authResult && authResult.accessToken && authResult.idToken) {
                            setSession(authResult);
                        }
                        else if (error) {
                            $window.location.href = _config.url.error; //'/home/error'
                        }
                        else if (service.isAuthenticated()) {
                            service.renew();
                        }
                        else {
                            service.logout();
                        }
                    });
                },

                login: function () {
                    angularAuth0.authorize();
                },

                logout: function () {
                    clearSession();
                    $location.url(_config.url.authCallback); //'/login'
                },

                isAuthenticated: function () {
                    let access_token = localStorage.getItem('access_token');
                    return access_token && jwtHelper.isTokenExpired(access_token) === false;
                },

                authProfile: function () {
                    let access_token = localStorage.getItem('access_token');
                    return $q(function (resolve, reject) {
                        if (access_token) {
                            angularAuth0.client.userInfo(access_token, function (error, profile) {
                                if (profile) {
                                    return resolve(profile);
                                }
                                else {
                                    reject(error || { error: 'user profile request failed.' });
                                }
                            });
                        }
                        else {
                            reject({ error: 'access token is missing.' });
                        }
                    });
                },

                renew: function () {
                    angularAuth0.renewAuth({
                        audience: angularAuth0.baseOptions.audience,
                        redirectUri: svcLib.uri(angularAuth0.baseOptions.redirectUri).replacePath(_config.url.authSilentCallback).href, //'silent'
                        usePostMessage: true
                    },
                        function (error, authResult) {
                            if (authResult) {
                                setSession(authResult);
                                notifySessionExpiry(new Date());
                            }
                            else {
                                svcLib.messageBox('Uwierzytelnianie', 'Wystąpił błąd podczas odświerzania tokena.', error || 'nieznany błąd.', true);
                            }
                        }
                    );
                },

                registerNotifySessionExpiryCallback: function (cb) {
                    if (cb && cb.constructor === Function) {
                        notifySessionExpiryCallback = cb;
                    }
                },

                configureXhr: function (jqXHR) {
                    if (jqXHR && jqXHR.constructor === Object && jqXHR.setRequestHeader != undefined) {
                        let access_token = localStorage.getItem('access_token');
                        if (access_token) {
                            jqXHR.setRequestHeader('Authorization', 'Bearer ' + access_token);
                        }
                    }
                },

                dispose: function () {
                    cancelSessionExpiryCheckout();
                }

            };

            function setSession(authResult) {
                let expiresAt = jwtHelper.getTokenExpirationDate(authResult.accessToken);

                localStorage.setItem('access_token', authResult.accessToken);
                localStorage.setItem('id_token', authResult.idToken);
                localStorage.setItem('expires_at', expiresAt.getTime());

                scheduleSessionExpiryCheck(expiresAt);
            }

            function clearSession() {
                localStorage.removeItem('access_token');
                localStorage.removeItem('id_token');
                localStorage.removeItem('expires_at');

                cancelSessionExpiryCheckout();
            }

            var notifySessionExpiryCallback = undefined;
            var scheduledSessionExpiryTimeout = undefined;
            var scheduledSessionExpiryInterval = undefined;
            var notifyIntervalMin = _config.renew.intervalWidthMin;
            var notifyIntervalCount = _config.renew.intervalDivCount;

            function scheduleSessionExpiryCheck(expiresAt) {
                cancelSessionExpiryCheckout();
                let now = new Date();
                var canSchedule = (expiresAt.getTime() - now.getTime()) > 0;
                if (canSchedule) {
                    var checkStartsBeforeExpiry = notifyIntervalMin * 60 * 1000;
                    let notifyEvery = checkStartsBeforeExpiry / notifyIntervalCount;
                    let scheduleAt = new Date(expiresAt.getTime() - checkStartsBeforeExpiry);
                    let scheduleFor = scheduleAt.getTime() - now.getTime();
                    let repeats = notifyIntervalCount;

                    if (scheduleFor < 0) {
                        scheduleFor = (-scheduleFor) % notifyEvery;
                        repeats -= Math.floor((-scheduleFor) / notifyEvery);
                    }

                    scheduledSessionExpiryTimeout = $timeout(function () {
                        notifySessionExpiry(expiresAt);

                        scheduledSessionExpiryInterval = $interval(function () {
                            notifySessionExpiry(expiresAt);
                        }, notifyEvery, repeats);

                        $timeout.cancel(scheduledSessionExpiryTimeout);
                        scheduledSessionExpiryTimeout = undefined;

                        scheduledSessionExpiryInterval.then(
                            function () {
                                finishSessionExpiryCheckout();
                            }, function () {
                                finishSessionExpiryCheckout();
                            });
                    }, scheduleFor);
                }
                else {
                    notifySessionExpiry(expiresAt);
                }
            }

            function finishSessionExpiryCheckout() {
                $interval.cancel(scheduledSessionExpiryInterval);
                scheduledSessionExpiryInterval = undefined;

                if (service.isAuthenticated() === false) {
                    $location.url(_config.url.authCallback); //'/login'
                }
            }

            function cancelSessionExpiryCheckout() {
                $timeout.cancel(scheduledSessionExpiryTimeout);
                scheduledSessionExpiryTimeout = undefined;
                $interval.cancel(scheduledSessionExpiryInterval);
                scheduledSessionExpiryInterval = undefined;
            }

            function notifySessionExpiry(expires) {
                if (notifySessionExpiryCallback) {
                    notifySessionExpiryCallback(expires);
                }
            }

            return service;
        }

        return provider;
    }

})();
