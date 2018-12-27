(function () {
    'use strict';

    angular.module('monitorLibrary', [])
        .factory('svcLib', svcLib);

    function svcLib() {

        var service = {

            messageBox: function (title, headText, bodyText, modal, bodyElements) {
                var dialog = $('<div title="Komunikat"><p></p></div>');
                if (title && title.length > 0) {
                    dialog.attr('title', title);
                }
                dialog.find('p').text(headText);
                if (bodyText && bodyText.length > 0) {
                    if (bodyElements === true) {
                        dialog.append(bodyText);
                    }
                    else {
                        dialog.append('<p></p>').children().last().text(bodyText);
                    }
                }
                dialog.ejDialog({ enableModal: modal === true ? true : false });
            },

            formatErrors: function (errors) {
                var result = '<p>format errors failed</p>';
                if (errors && errors.length > 0) {
                    result = errors.map(function (e) {
                        return '<p>' + e + '</p>';
                    }).join('');
                }
                return $(result);
            },

            condense: function (string) {
                if (string) {
                    return string.replace(/\s/g, '');
                }
                else {
                    return string;
                }
            },

            isNumber: function (string) {
                return /^\d+$/g.test(string);
            },

            uri: function (value) {
                configureAnchor();

                if (value && value.constructor === String) {
                    return $('<a>').attr('href', value)[0];
                }
                else {
                    return $('<a>')[0];
                }

                function configureAnchor() {
                    if (HTMLAnchorElement.prototype.replacePath === undefined) {
                        HTMLAnchorElement.prototype.replacePath = function (path) {
                            this.pathname = path;
                            return this;
                        };
                    }
                }
            }

        };

        return service;
    }

})();

