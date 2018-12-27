(function () {

    'use strict';

    angular.module('monitor')
        .controller('monitor', monitorCtrl);

    function monitorCtrl($scope, $timeout, svcAuth, svcLib, svcApi, userFieldHeaders, scopes) {
        $timeout(init, 0);
        
        function init() {
            registerDestroyHandler();
            configurejQueryAjaxEvents();

            createTabs();
            createActive();
            createExpired();
            createVersion();
            createAddForm();
            createRecent();
        }
        

        function configurejQueryAjaxEvents() {
            $(document).ajaxSend(function (event, jqXHR, options) {
                svcAuth.configureXhr(jqXHR);
            });
        }

        function registerDestroyHandler() {
            $scope.$on('$destroy', function () {
                svcAuth.dispose();
            });
        }

        function createTabs() {
            $('#tabLicenses').ejTab({
                itemActive: function (arg) {
                    var gridElement = $(this.contentPanels[arg.activeIndex]).find('div.e-grid');
                    if (gridElement.length > 0) {
                        gridElement.data('ejGrid').refreshContent();
                    }
                }
            });
        }

        function createActive() {
            $('#gridActive').ejGrid({
                dataSource: ej.DataManager({ url: svcApi.url.license.active, adaptor: 'UrlAdaptor', crudUrl: svcApi.url.license.edit }),
                columns: [{
                    field: 'userId',
                    isPrimaryKey: true,
                    allowEditing: false,
                    headerText: 'User Id',
                    type: 'number'
                }, {
                    field: 'nick',
                    headerText: 'Nick',
                    type: 'string'
                }, {
                    field: 'validTo',
                    headerText: 'Valid To',
                    type: 'datetime',
                    format: '{0:ddd, d MMMM yyyy, HH:mm zz}',
                    editType: 'datetimepicker',
                    editParams: {
                        locale: 'pl-PL',
                        allowEdit: false,
                        maxDateTime: new Date(9999, 12, 31, 23, 59, 0, 0),
                        dateTimeFormat: 'yyyy-MM-ddTHH:mmzz',
                        timeDisplayFormat: 'HH:mm',
                        timeDrillDown: { enabled: true, interval: 15 },
                        showOtherMonths: false,
                    }
                }, {
                    field: 'scope',
                    headerText: 'Scope',
                    type: 'string',
                    template: '{{:scope}}',
                    editTemplate: {
                        create: function () {
                            return '<input>';
                        },
                        read: function (arg) {
                            return readScopeEdit(arg);
                        },
                        write: function (arg) {
                            createScopeEdit(arg);
                        }
                    }
                }, {
                    field: 'note',
                    headerText: 'Note',
                    type: 'string',
                    clipMode: 'ellipsiswithtooltip',
                    tooltip: '{{:value}}'
                }],
                allowSorting: true,
                allowPaging: true,
                allowSearching: true,
                sortSettings: { sortedColumns: [{ field: 'nick', direction: 'ascending' }] },
                pageSettings: { pageSize: 20 },
                searchSettings: { fields: ['nick'], operator: 'contains', ignoreCase: true },
                editSettings: { allowEditing: true },
                toolbarSettings: { showToolbar: true, toolbarItems: ['edit', 'update', 'cancel', 'search'] },
                detailsTemplate: '#gridRowDetails',
                detailsDataBound: gridDetailsDataBound,
                actionFailure: gridActionFailure,
                templateRefresh: gridTemplateRefresh
            });
        }

        function createExpired() {
            $('#gridExpired').ejGrid({
                dataSource: ej.DataManager({ url: svcApi.url.license.expired, adaptor: 'UrlAdaptor', crudUrl: svcApi.url.license.edit }),
                columns: [{
                    field: 'userId',
                    isPrimaryKey: true,
                    allowEditing: false,
                    headerText: 'User Id',
                    type: 'number'
                }, {
                    field: 'nick',
                    headerText: 'Nick',
                    type: 'string'
                }, {
                    field: 'validTo',
                    headerText: 'Valid To',
                    type: 'datetime',
                    format: '{0:ddd, d MMMM yyyy, HH:mm zz}',
                    editType: 'datetimepicker',
                    editParams: {
                        locale: 'pl-PL',
                        allowEdit: false,
                        maxDateTime: new Date(9999, 12, 31, 23, 59, 0, 0),
                        dateTimeFormat: 'yyyy-MM-ddTHH:mmzz',
                        timeDisplayFormat: 'HH:mm',
                        timeDrillDown: { enabled: true, interval: 15 },
                        showOtherMonths: false,
                    }
                }, {
                    field: 'scope',
                    headerText: 'Scope',
                    type: 'string',
                    template: '{{:scope}}',
                    editTemplate: {
                        create: function () {
                            return '<input>';
                        },
                        read: function (arg) {
                            return readScopeEdit(arg);
                        },
                        write: function (arg) {
                            createScopeEdit(arg);
                        }
                    }
                }, {
                    field: 'note',
                    headerText: 'Note',
                    type: 'string',
                    clipMode: 'ellipsiswithtooltip',
                    tooltip: '{{:value}}'
                }],
                allowSorting: true,
                allowPaging: true,
                allowSearching: true,
                sortSettings: { sortedColumns: [{ field: 'nick', direction: 'ascending' }] },
                pageSettings: { pageSize: 20 },
                searchSettings: { fields: ['nick'], operator: 'contains', ignoreCase: true },
                editSettings: { allowEditing: true, allowDeleting: true, showDeleteConfirmDialog: true },
                toolbarSettings: { showToolbar: true, toolbarItems: ['edit', 'delete', 'update', 'cancel', 'search'] },
                detailsTemplate: '#gridRowDetails',
                detailsDataBound: gridDetailsDataBound,
                actionFailure: gridActionFailure,
                templateRefresh: gridTemplateRefresh
            });
        }

        function createVersion() {
            $('#gridVersion').ejGrid({
                dataSource: ej.DataManager({ url: svcApi.url.version.list, adaptor: 'UrlAdaptor', crudUrl: svcApi.url.version.edit }),
                columns: [{
                    field: 'validFrom',
                    isPrimaryKey: true,
                    headerText: 'Valid From',
                    type: 'datetime',
                    format: '{0:ddd, d MMMM yyyy, HH:mm zz}',
                    editType: 'datetimepicker',
                    editParams: {
                        locale: 'pl-PL',
                        allowEdit: false,
                        dateTimeFormat: 'yyyy-MM-ddTHH:mmzz',
                        timeDisplayFormat: 'HH:mm',
                        timeDrillDown: { enabled: true, interval: 15 },
                        showOtherMonths: false,
                    }
                }, {
                    field: 'validTo',
                    headerText: 'Valid To',
                    type: 'datetime',
                    allowEditing: false,
                    format: '{0:ddd, d MMMM yyyy, HH:mm zz}'
                }, {
                    field: 'requiredAtLeast',
                    headerText: 'Required at Least',
                    type: 'string'
                }],
                allowSorting: true,
                allowPaging: true,
                sortSettings: { sortedColumns: [{ field: 'validTo', direction: 'descending' }] },
                pageSettings: { pageSize: 20 },
                editSettings: { allowAdding: true, allowDeleting: true, showDeleteConfirmDialog: true },
                toolbarSettings: { showToolbar: true, toolbarItems: ['add', 'delete', 'update', 'cancel'] },
                actionBegin: gridVersionActionBegin,
                actionComplete: gridVersionActionComplete,
                actionFailure: gridActionFailure
            });
        }

        function createRecent() {
            $('#gridRecent').ejGrid({
                dataSource: ej.DataManager({ url: svcApi.url.license.recent, adaptor: 'UrlAdaptor' }),
                columns: [{
                    field: 'user',
                    headerText: 'User Id',
                    type: 'number'
                }, {
                    field: 'nick',
                    headerText: 'Nick',
                    type: 'string'
                }, {
                    field: 'date',
                    headerText: 'Date',
                    type: 'datetime',
                    format: '{0:ddd, d MMMM yyyy, HH:mm zz}'
                }, {
                    field: 'version',
                    headerText: 'Version',
                    type: 'string'
                }],
                allowSorting: true,
                sortSettings: { sortedColumns: [{ field: 'date', direction: 'descending' }] }
            });
        }

        function createAddForm() {
            $scope.data = { nick: '', validto: '', scope: '', note: '' };

            var changed = false;
            $('#tbxNick').ejAutocomplete({
                dataSource: [],
                showPopupButton: true,
                showResetIcon: true,
                showLoadingIcon: false,
                showEmptyResultText: false,
                delaySuggestionTimeout: 0,
                width: 200,
                actionBegin: function (arg) {
                    if (changed === true) {
                        changed = false;
                    }
                    else {
                        gridUser.hide();
                        validateUser($scope.data, showUser, waitPop, gridUser);
                    }
                },
                change: function (arg) {
                    if (arg.value) {
                        changed = true;
                    }
                    else {
                        angular.element(this.element).triggerHandler('input');
                    }
                }
            })

            $('#tbxValidTo').ejDateTimePicker({
                locale: 'pl-PL',
                allowEdit: false,
                maxDateTime: new Date(9999, 12, 31, 23, 59, 0, 0),
                dateTimeFormat: 'yyyy-MM-ddTHH:mmzz',
                timeDisplayFormat: 'HH:mm',
                timeDrillDown: { enabled: true, interval: 15 },
                showOtherMonths: false,
                width: 200,
                change: function (arg) {
                    if (arg.isValidState === true && arg.isInteraction === true) {
                        angular.element(this.element).triggerHandler('input');
                    }
                }
            });

            var scopeItems = convertScopeToList(0);
            $('#tbxScope').ejDropDownList({
                dataSource: scopeItems,
                showCheckbox: true,
                fields: { value: 'value', text: 'text', selected: 'selected' },
                width: 200,
                change: function (arg) {
                    angular.element(this.element).triggerHandler('input');
                }
            });

            $('#btnAdd').ejButton({
                text: 'Add',
                click: function (arg) {
                    gridUser.hide();
                    validateUser($scope.data, addLicense, waitPop, undefined);
                }
            });

            var gridUser = $('#gridUser').ejGrid({
                dataSource: [],
                columns: [{
                    field: 'key',
                    type: 'string',
                    visible: false
                }, {
                    field: 'header',
                    type: 'string',
                    width: 150
                }, {
                    field: 'value',
                    type: 'string',
                    template: '#gridUserValueColumn'
                }],
                enableAltRow: false,
                create: function (arg) {
                    this.__proto__.show = function () {
                        this.element.show();
                    };
                    this.__proto__.hide = function () {
                        this.element.hide();
                    };
                    this.hide();
                }
            }).data('ejGrid');

            var waitPop = $('#waitPopup').ejWaitingPopup({
                showOnInit: false,
                target: '#tabAdd'
            }).data('ejWaitingPopup');
        }

        function validateUser(formdata, onuservalid, wait, control) {
            wait.show();

            var data = { userid: undefined, nick: formdata.nick, validto: formdata.validto, scope: formdata.scope, note: formdata.note };

            data.nick = svcLib.condense(data.nick);
            if (svcLib.isNumber(data.nick)) {
                data.userid = data.nick;
                data.nick = undefined;
            }

            svcApi.allegroUser(data.userid, data.nick)
                .then(function (response) {
                    data.userid = response.data.id;
                    data.nick = response.data.login;

                    onuservalid(data, response.data, wait, control);
                }
                , function (error) {
                    wait.hide();
                    if (error.data.errors) {
                        svcLib.messageBox('Użytkownik', 'Nie znaleziono użytkownika ' + data.nick + ' id: ' + data.userid + '.', svcLib.formatErrors(error.data.errors), true, true);
                    }
                    else {
                        svcLib.messageBox('Użytkownik', 'Nie znaleziono użytkownika ' + data.nick + ' id: ' + data.userid + '.', error.status + ' ' + error.statusText, true);
                    }
                });
        }

        function addLicense(formdata, userdata, wait, control) {
            formdata.scope = convertListToScope(formdata.scope);

            svcApi.addLicense(formdata)
                .then(
                function (response) {
                    wait.hide();
                    svcLib.messageBox('Nowa licencja', 'Dodano licencję dla użytkownika ' + response.data.nick + ' id: ' + response.data.userId + '.', undefined, true);
                },
                function (error) {
                    wait.hide();
                    if (error.data.errors) {
                        svcLib.messageBox('Nowa licencja', 'Wystąpił błąd podczas zapisu do bazy danych.', svcLib.formatErrors(error.data.errors), true, true);
                    }
                    else {
                        svcLib.messageBox('Nowa licencja', 'Wystąpił błąd podczas zapisu do bazy danych.', error.status + ' ' + error.statusText, true);
                    }
                });
        }

        function showUser(formdata, userdata, wait, control) {
            var grid = control;
            var data = [];

            for (var key in userdata) {
                var header = userFieldHeaders.data[key];
                data.push({ key: key, header: header, value: userdata[key] });
            }

            grid.dataSource(data);
            grid.show();
            wait.hide();
        }

        function refreshFormData(form) {
            form.find('input[type="text"], textarea').each(function (i, e) {
                $(e).trigger('keydown');
            });
        }

        function gridVersionActionBegin(arg) {
            if (arg.requestType === 'save') {
                arg.data.validTo = new Date('9999-12-31');
            }
            else if (arg.requestType === 'delete') {
                var grid = this;
                arg.cancel = true;

                svcApi.removeVersion(arg.data)
                    .then(
                    function (response) {
                        grid.refreshContent();
                    },
                    function (error) {
                        svcLib.messageBox('Usuwanie', 'Wystąpił błąd podczas zapisu do bazy danych.', svcLib.formatErrors(error.data.errors), true, true);
                    });
            }
        }

        function gridVersionActionComplete(arg) {
            if (arg.requestType === 'save') {
                this.refreshContent();
            }
        }

        function gridActionFailure(arg) {
            if (arg.requestType === 'save' && (arg.action === 'edit' || arg.action === 'add')) {
                svcLib.messageBox('Edycja', 'Wystąpił błąd podczas zapisu do bazy danych.', svcLib.formatErrors(arg.error.responseJSON.errors), true, true);
            }
            else if (arg.requestType === 'delete') {
                svcLib.messageBox('Usuwanie', 'Wystąpił błąd podczas zapisu do bazy danych.', svcLib.formatErrors(arg.error.error.responseJSON.errors), true, true);
            }
            else if (arg.requestType === 'refresh') {
                svcLib.messageBox('Odczyt', 'Wystąpił błąd podczas odczytu danych.', arg.error.status + ' ' + arg.error.statusText, true);
            }
            else if (!arg.requestType && arg.error) {
                if (arg.error.responseJSON) {
                    svcLib.messageBox('Odczyt', 'Wystąpił błąd podczas odczytu danych.', svcLib.formatErrors(arg.error.responseJSON.errors), true, true);
                }
                else {
                    svcLib.messageBox('Odczyt', 'Wystąpił błąd podczas odczytu danych.', arg.error.status + ': ' + arg.error.responseText, true);
                }
            }
            else {
                svcLib.messageBox('Odczyt', 'Wystąpił nieznany błąd podczas odczytu danych.', undefined, true);
            }
        }

        function gridTemplateRefresh(arg) {
            displayScope(arg.cell, arg.data);
        }

        function displayScope(cell, data) {
            if (cell && data) {
                if (data.scope > 0) {
                    var userScope = [];
                    for (var i = 0; i < scopes.data.count; ++i) {
                        var scopeElement = scopes.data.scopes[i];
                        if ((data.scope & scopeElement.value) === scopeElement.value) {
                            userScope.push(scopeElement.name);
                        }
                    }
                    cell.innerText = userScope.join(', ');
                }
                else {
                    cell.innerText = scopes.data.emptyName;
                }
            }
        }

        function createScopeEdit(arg) {
            var items = convertScopeToList(arg.rowdata.scope);

            arg.element.ejDropDownList({
                dataSource: items,
                showCheckbox: true,
                fields: { value: 'value', text: 'text', selected: 'selected' }
            });
        }

        function readScopeEdit(arg) {
            var selected = arg.data('ejDropDownList').getSelectedValue();
            return convertListToScope(selected);
        }

        function convertScopeToList(scopeToSelect) {
            return scopes.data.scopes.map(function (i) {
                return { value: i.value, text: i.name, selected: (scopeToSelect & i.value) === i.value };
            });
        }

        function convertListToScope(selected) {
            if (selected) {
                return selected.split(',').reduce(function (sum, current) {
                    return parseInt(sum) + parseInt(current);
                });
            }
            else {
                return 0;
            }
        }

        function gridDetailsDataBound(arg) {
            var userId = arg.data.userId;

            var tabElement = $(arg.detailsElement).find('.gridRowDetailsTab');
            if (tabElement) {
                tabElement.ejTab();
            }

            var gridAuditElement = $(arg.detailsElement).find('.detailsGridAudit');
            if (gridAuditElement) {
                var gridAudit = gridAuditElement.ejGrid({
                    dataSource: [],
                    columns: [{
                        field: 'id',
                        headerText: 'Id',
                        isPrimaryKey: true,
                        visible: false
                    }, {
                        field: 'date',
                        headerText: 'Change date',
                        type: 'datetime',
                        format: '{0:ddd, d MMMM yyyy, HH:mm zz}'
                    }, {
                        field: 'validTo',
                        headerText: 'Valid To',
                        type: 'datetime',
                        format: '{0:ddd, d MMMM yyyy, HH:mm zz}'
                    }, {
                        field: 'scope',
                        headerText: 'Scope',
                        type: 'string',
                        template: '{{:scope}}'
                    }, {
                        field: 'note',
                        headerText: 'Note',
                        type: 'string',
                        clipMode: 'ellipsiswithtooltip',
                        tooltip: '{{:value}}'
                    }],
                    allowSorting: true,
                    allowMultiSorting: true,
                    enableAltRow: false,
                    sortSettings: { sortedColumns: [{ field: 'date', direction: 'descending' }] },
                    templateRefresh: gridTemplateRefresh,
                    create: function (arg) {
                        svcApi.userLicenseAudit(userId)
                            .then(
                            function (response) {
                                gridAudit.dataSource(ej.parseJSON(response.data));
                            },
                            function (error) {
                            });
                    }
                }).data('ejGrid');
            }

            var gridActivityElement = $(arg.detailsElement).find('.detailsGridActivity');
            if (gridActivityElement) {
                var gridActivity = gridActivityElement.ejGrid({
                    dataSource: [],
                    columns: [{
                        field: 'id',
                        headerText: 'Id',
                        isPrimaryKey: true,
                        visible: false
                    }, {
                        field: 'date',
                        headerText: 'Date',
                        type: 'datetime',
                        format: '{0:ddd, d MMMM yyyy, HH:mm zz}'
                    }, {
                        field: 'appVersion',
                        headerText: 'Version',
                        type: 'string'
                    }],
                    allowSorting: true,
                    allowMultiSorting: true,
                    enableAltRow: false,
                    sortSettings: { sortedColumns: [{ field: 'date', direction: 'descending' }] },
                    create: function (arg) {
                        svcApi.userActivity(userId)
                            .then(
                            function (response) {
                                gridActivity.dataSource(ej.parseJSON(response.data));
                            },
                            function (error) {
                            });
                    }
                }).data('ejGrid');
            }
        }
    }

})();


$(document).ready(function () {
    $.views.helpers({
        writeUserValue: function (key, data) {
            if (data.value !== undefined && data.value.constructor === Boolean) {
                return data.value ? '<a class="e-icon e-checkmark"></a>' : '<a class="e-icon e-close_01"></a>';
            }

            if (data.key === 'country' && data.value === 1) {
                return 'Polska';
            }

            if (data.key === 'created' || data.key === 'lastLogin') {
                var options = { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit', weekday: 'long', timeZoneName: 'short' };
                return Intl.DateTimeFormat('pl-PL', options).format(new Date(data.value));
            }

            return data.value;
        }
    });
});