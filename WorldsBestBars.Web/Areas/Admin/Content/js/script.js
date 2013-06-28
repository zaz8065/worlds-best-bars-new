$('body').ajaxComplete(function (event, xhr, opt) {
    if (xhr.status == 302) {
        Helpers.alertDialog('It looks like you\'ve been logged out.  Please login again', function () {
            window.location = '/admin/auth/login';
        });
    }
});

$.ajax({
    url: site_base + 'admin/dialogs/menu'
}).success(function (data) {
    $(data).dialog({
        title: 'Worlds Best Bars Administration',
        draggable: false,
        resizable: false,
        closeOnEscape: false,
        position: [20, 20],
        open: function (event, ui) { $(".ui-dialog-titlebar-close", $(this).parent()).hide(); }
    });
});

$('.action').live('click', function (e) {
    e.preventDefault();

    wbb.executeAction($(this).attr('data-action'), this);
});

var WBB = function () {
    this._actions = [];
};

WBB.prototype.addAction = function (action, handler) {
    var _handler = new handler();

    if (_handler.init) {
        _handler.init();
    }

    this._actions.push({
        'name': action,
        'handler': _handler
    });
};

WBB.prototype.executeAction = function (action, source) {
    for (var i = 0; i < this._actions.length; i++) {
        var _action = this._actions[i];
        if (_action.name == action) {
            return _action.handler.execute(source);
        }
    }

    Helpers.alertDialog('Sorry, I don\'t know what that action is: ' + action);
};

var wbb = new WBB();

wbb.addAction('edit-bar', CreateBarAction);
wbb.addAction('manage-locations', ManageLocationsAction);
wbb.addAction('manage-experts', ManageExpertsAction);
wbb.addAction('manage-lists', ManageListsAction);
wbb.addAction('manage-documents', ManageDocumentsAction);
wbb.addAction('manage-advertising', ManageAdvertsAction);
wbb.addAction('moderate-reviews', ModerateReviewsAction);
wbb.addAction('moderate-bar-resource-submissions', ModerateBarResourceSubmissionsAction);

var Helpers = {
    confirmDialog: function (text, callback) {
        text = text || 'Are you sure?';
        $('<p><span class="ui-icon ui-icon-alert" style="float:left; margin:0 7px 20px 0;"></span>' + text + '</p>').dialog({
            modal: true,
            title: 'Confirmation',
            open: function (event, ui) { $(".ui-dialog-titlebar-close", $(this).parent()).hide(); },
            close: function (event, ui) { $(this).remove(); },
            draggable: false,
            resizable: false,
            show: 'fade',
            hide: 'fade',
            buttons: {
                'Yes': function () { $(this).dialog('destroy'); $(this).remove(); if (callback) { callback('yes') }; },
                'No': function () { $(this).dialog('destroy'); $(this).remove(); if (callback) { callback('no') }; }
            }
        });
    },

    alertDialog: function (text, callback) {
        text = text || 'Hello.';
        $('<p>' + text + '</p>').dialog({
            modal: true,
            title: '<span class="ui-icon ui-icon-info"></span> Alert',
            open: function (event, ui) { $(".ui-dialog-titlebar-close", $(this).parent()).hide(); },
            close: function (event, ui) { $(this).remove(); },
            draggable: false,
            resizable: false,
            show: 'fade',
            hide: 'fade',
            buttons: {
                'OK': function () { $(this).dialog('destroy'); $(this).remove(); if (callback) { callback('yes') }; }
            }
        });
    },

    inputDialog: function (prompt, callback) {
        prompt = prompt || 'Hello.';
        $('<div><label>' + prompt + '</label><input type="text" class="text ui-widget-content ui-corner-all" /></div>').dialog({
            modal: true,
            title: 'Input',
            open: function (event, ui) { $(".ui-dialog-titlebar-close", $(this).parent()).hide(); },
            close: function (event, ui) { $(this).remove(); },
            draggable: false,
            resizable: false,
            show: 'fade',
            hide: 'fade',
            buttons: {
                'OK': function () {
                    var value = $('input', this).val();
                    $(this).dialog('destroy');
                    $(this).remove();
                    if (callback) {
                        callback(true, value)
                    };
                },
                'Cancel': function () {
                    $(this).dialog('destroy');
                    $(this).remove();
                    if (callback) {
                        callback(false, '')
                    };
                }
            }
        });
    }
};

if (location.hash.length > 0) {
    var action = location.hash.substr(1);
    wbb.executeAction(action);
}