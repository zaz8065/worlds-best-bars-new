var global = global || {};

global.documents = [];

var getDocument = function (id, list) {
    var that = this;

    list = list || global.documents[0].children;

    var ret = false;

    $(list).each(function (index, document) {
        if (document.metadata.id == id) {
            ret = document;
        }
        else {
            ret = getDocument(id, document.children);
        }
        if (ret) {
            return false;
        }
    });

    return ret;
};

var ManageDocumentsAction = function () {
    var that = this;

    this.dialog = false;
    this.tabs = false;
    this.tree = false;
    this.selectedNode = false;
    this.activateButton = false;
};

ManageDocumentsAction.prototype.init = function () {
    var that = this;

    var initDialog = function () {
        $.ajax({
            url: site_base + 'admin/dialogs/managedocuments'
        }).success(function (data) {
            $(data).dialog({
                autoOpen: false,
                title: 'Manage documents',
                width: 800,
                height: 600,
                resizable: true,
                show: 'fade',
                hide: 'fade',
                create: function (event, ui) {
                    that.dialog = this;

                    var rand = parseInt(Math.random() * 1000);
                    that.tabs = $('.right', that.dialog);

                    $('ul li.tab-head', that.tabs).each(function (index, el) {
                        var target = $('a', el).attr('href').substr(1);

                        var panel = $('.' + target, that.tabs);

                        $(panel).attr('id', rand + target);
                        $('a', el).attr('href', '#' + rand + target);
                    });

                    that.tabs.tabs();

                    $('.left button.refresh').button({ icons: { primary: "ui-icon-refresh" }, text: false }).click(function (e) { e.preventDefault(); that.reloadData(); });

                    $('.right button.preview').button({ icons: { primary: 'ui-icon-search' }, text: false }).click(function (e) { e.preventDefault(); that.previewContent(); });
                    $('.right button.format').button({ icons: { primary: 'ui-icon-check' }, text: false }).click(function (e) { e.preventDefault(); that.formatContent(); });
                    $('.right button.save').button().click(function (e) { e.preventDefault(); that.saveDocument(); });
                    $('.right button.activate').button().click(function (e) { e.preventDefault(); that.activateDocument(); });
                    that.activateButton = $('.right button.activate');

                    that.activateButton.button('option', 'disabled', true);

                    $('.related .existing', that.dialog).jScrollPane({ autoReinitialise: true });

                    $('.related input[name=search]', that.dialog).keyup(function (e) {
                        var theother = this;
                        clearTimeout($(this).data('search-timer'));
                        $(this).data('search-timer', setTimeout(function () { that.relatedSearch($(theother).val()); }, 500));
                    });

                    $('a.redirect', that.dialog).click(function (e) { e.preventDefault(); that.selectRedirect(); });

                    $('input[type=date]', that.dialog).datepicker({ dateFormat: 'yy-M-dd' });

                    that.reloadData();
                },
                buttons: {
                    'Close': function () {
                        $(that.dialog).dialog('close');
                    }
                }
            });
        });
    };

    initDialog();
};

ManageDocumentsAction.prototype.execute = function () {
    $(this.dialog).dialog('open');
};

ManageDocumentsAction.prototype.createDocument = function (e, data) {
    var that = this;

    var request_data = {
        name: data.rslt.name,
        parent: data.rslt.parent.data('id')
    };

    $.ajax({
        url: site_base + 'admin/process/createdocument',
        data: request_data,
        type: 'POST',
        success: function (response_data) {
            if (response_data != 'false') {
                data.rslt.obj.attr('id', response_data);

                data.rslt.obj.data('id', response_data);
                $(data.rslt.obj).data('id', response_data);


                var parent = getDocument(request_data.parent) || global.documents[0];
                parent.children.push({
                    children: [],
                    data: request_data.name,
                    metadata: {
                        id: response_data,
                        active: false,
                        content: '',
                        name: request_data.name,
                        synopsis: '',
                        urlkey: ''
                    }
                });
            }
            else {
                $.jstree.rollback(data.rlbk);
            }
        },
        error: function (data) {
            $.jstree.rollback(data.rlbk);
        }
    });
};

ManageDocumentsAction.prototype.deleteDocument = function (e, data) {
    var id = data.rslt.obj.data('id');
    if (id) {
        Helpers.confirmDialog('Are you sure you want to delete this document?  You will not be able to delete documents that have children.', function (response) {
            if (response == 'yes') {
                Helpers.confirmDialog('Are you REALLY sure you want to delete this document?', function (response2) {
                    if (response2 == 'yes') {
                        $.ajax({
                            url: site_base + 'admin/process/deletedocument/' + id,
                            success: function (data) {
                                if (data == 'false') {
                                    Helpers.alertDialog('Something went wrong.');
                                }
                                else {
                                    $.jstree.rollback(data.rlbk);
                                }
                            },
                            error: function (data) {
                                Helpers.alertDialog('Something went wrong.');
                                $.jstree.rollback(data.rlbk);
                            }
                        });
                    }
                });
            }
        });
    }
};

ManageDocumentsAction.prototype.renameDocument = function (e, data) {
    var id = data.rslt.obj.data('id');

    $.ajax({
        url: site_base + 'admin/process/renamedocument/' + id,
        type: 'POST',
        data: { name: data.rslt.new_name },
        success: function (response_data) {
            if (response_data == 'false') {
                $.jstree.rollback(data.rlbk);

                Helpers.alertDialog('Something went wrong.');
            }
        },
        error: function (data) {
            $.jstree.rollback(data.rlbk);
        }
    });
};

ManageDocumentsAction.prototype.moveDocument = function (e, data) {
    var node = data.rslt.o;
    var new_parent = data.rslt.np;

    if (!new_parent.data('id')) {
        $.jstree.rollback(data.rlbk);
        return;
    }

    var source = { id: node.data('id'), name: node.data('name') };
    var destination = { id: new_parent.data('id'), name: new_parent.data('name') };

    Helpers.confirmDialog('Are you sure you want to move "' + source.name + '" to "' + destination.name + '"?', function (response) {
        if (response == 'yes') {
            $.ajax({
                url: site_base + 'admin/process/movedocument/' + source.id,
                data: { parent: destination.id },
                success: function (data) {
                    if (data == 'false') {
                        $.jstree.rollback(data.rlbk);
                    }
                },
                error: function (data) {
                    $.jstree.rollback(data.rlbk);
                }
            });
        }
        else {
            $.jstree.rollback(data.rlbk);
        }
    });
};

ManageDocumentsAction.prototype.loadDocument = function (e, data) {
    var that = this;

    var id = data.rslt.obj.data('id');

    that.loadRelated(id);
    that.loadMedia(id);

    $('.upload-form', that.dialog).bind('submit', function () {

        $('.upload-form', that.dialog).attr('action', '/admin/process/uploadmedia/' + id + '?process=' + ($('input[name=process]', that.dialog).is(':checked') ? 'true' : 'false'));
        return AIM.submit(this, {
            'onStart': function () {
            },
            'onComplete': function () {
                that.loadMedia(id);
            }
        });
    });

    var _document = getDocument(id);

    if (id) {
        that.activateButton.button('option', 'disabled', false);

        var document = {
            id: _document.metadata.id,
            name: _document.metadata.name,
            synopsis: _document.metadata.synopsis,
            content: _document.metadata.content,
            urlkey: _document.metadata.urlkey,
            redirect: _document.metadata.redirect,
            created: new Date(_document.metadata.created)
        };

        that.currentDocument = _document;

        $('input[type=date]', that.dialog).datepicker('setDate', document.created);
        $('input[name=urlkey]', that.dialog).val(document.urlkey);
        $('textarea[name=content]', that.dialog).val(document.content);
        $('textarea[name=synopsis]', that.dialog).val(document.synopsis);
        if (document.redirect == null) {
            $('input[name=redirect]', that.dialog).val('').attr('data-id', '');
        }
        else {
            $('input[name=redirect]', that.dialog).val(document.redirect.url).attr('data-id', document.redirect.id);
        }

        that.activateButton.button('option', 'label', _document.metadata.active ? 'Deactivate' : 'Activate');
    }
    else {
        that.activateButton.button('option', 'disabled', true);

        $('input[type=date]', that.dialog).datepicker('setDate', null);
        $('input[name=urlkey]', that.dialog).val('');
        $('textarea[name=content]', that.dialog).val('');
        $('textarea[name=synopsis]', that.dialog).val('');

        that.currentDocument = false;
    }
};

ManageDocumentsAction.prototype.saveDocument = function (event, ui) {
    var that = this;

    if (that.currentDocument) {
        var submit_data = {
            name: that.currentDocument.metadata.name,
            synopsis: $('textarea[name=synopsis]', that.dialog).val(),
            content: $('textarea[name=content]', that.dialog).val(),
            urlkey: $('input[name=urlkey]', that.dialog).val(),
            created: $.datepicker.formatDate('yy-mm-dd', $('input[type=date]', that.dialog).datepicker('getDate'))
        }

        $.ajax({
            url: site_base + 'admin/process/editdocument/' + that.currentDocument.metadata.id,
            data: submit_data,
            type: 'POST',
            success: function (response_data) {
                if (response_data != 'false') {
                    that.currentDocument.metadata.synopsis = submit_data.synopsis;
                    that.currentDocument.metadata.content = submit_data.content;
                    that.currentDocument.metadata.urlkey = submit_data.urlkey;
                    that.currentDocument.metadata.created = $('input[type=date]', that.dialog).datepicker('getDate');
                }
                else {
                    Helpers.alertDialog('Something went wrong.');
                }
            },
            error: function (response_data) {
                Helpers.alertDialog('Something went wrong.');
            }
        });
    }
};

ManageDocumentsAction.prototype.activateDocument = function () {
    var that = this;

    Helpers.confirmDialog('Are you sure you want to ' + (that.currentDocument.metadata.active ? 'de' : '') + 'activate this document?', function (response) {
        if (response == 'yes') {

            var submit_data = {
                active: !that.currentDocument.metadata.active
            };

            $.ajax({
                url: site_base + 'admin/process/toggledocumentactivation/' + that.currentDocument.metadata.id,
                data: submit_data,
                type: 'POST'
            }).success(function (data) {
                if (data != 'true') {
                    Helpers.alertDialog('Something went wrong.');
                }
                else {
                    that.currentDocument.metadata.active = submit_data.active;

                    that.activateButton.button('option', 'label', submit_data.active ? 'Deactivate' : 'Activate');
                }
            });
        }
    });
};

ManageDocumentsAction.prototype.loadRelated = function (id) {
    var that = this;
    if (!id) { return; }
    $.ajax({
        url: site_base + 'admin/json/getrelated/' + id,
        success: function (data) {
            var container = $('.related .existing .search-results', that.dialogDom);

            container.empty();

            $(data).each(function (index, element) {
                var result = $('<div class="result" data-id="' + element.id + '"><span class="name">' + element.name + '</span><span class="type">' + element.type + '</span></div>');
                container.append(result);

                $(result).click(function (e) { e.preventDefault(); that.removeRelated($(this).attr('data-id'), this); });
            });
        }
    });
};

ManageDocumentsAction.prototype.relatedSearch = function (query) {
    var that = this;
    $.ajax({
        url: site_base + 'admin/json/search',
        data: { search: query, limit: 15 },
        success: function (data) {
            var container = $('.related .add .search-results', that.dialogDom);

            container.empty();

            $(data).each(function (index, element) {
                var result = $('<div class="result" data-id="' + element.id + '"><span class="name">' + element.name + '</span><span class="type">' + element.type + '</span></div>');
                container.append(result);

                $(result).click(function (e) { e.preventDefault(); that.addRelated($(this).attr('data-id'), this); });
            });
        }
    });
};

ManageDocumentsAction.prototype.addRelated = function (id, elem) {
    var that = this;
    if (that.loading) { return; }
    that.loading = true;
    $.ajax({
        url: site_base + 'admin/process/AddRelated/' + that.currentDocument.metadata.id,
        data: { right: id },
        success: function (data) {
            if (data != 'false') {
                $(elem).detach().appendTo($('.related .existing .search-results', that.dialogDom));
                $(elem).unbind('click');
                $(elem).click(function (e) { e.preventDefault(); that.removeRelated(id, elem); });
            }
            else {
                Helpers.alertDialog('Something went wrong.');
            }

            that.loading = false;
        },
        error: function () {
            Helpers.alertDialog('Something went wrong.');

            that.loading = false;
        }
    });
};

ManageDocumentsAction.prototype.removeRelated = function (id, elem) {
    var that = this;
    if (that.loading) { return; }
    that.loading = true;
    $.ajax({
        url: site_base + 'admin/process/RemoveRelated/' + that.currentDocument.metadata.id,
        data: { right: id },
        success: function (data) {
            if (data != 'false') {
                $(elem).remove();
            }
            else {
                Helpers.alertDialog('Something went wrong.');
            }

            that.loading = false;
        },
        error: function () {
            Helpers.alertDialog('Something went wrong.');

            that.loading = false;
        }
    });
};

ManageDocumentsAction.prototype.loadMedia = function (id) {
    var that = this;
    $.ajax({
        url: site_base + 'admin/json/getmedia/' + id,
        dataType: 'json',
        success: function (data) {
            $('.media-list', that.dialog).empty();
            $(data).each(function (index, url) {
                var dom = $('<div class="media"><a href="' + url + '" rel="nyro"><img src="' + url + '" /></a><div class="actions"><button class="delete">Delete</button></div>');
                $('.media-list', that.dialog).append(dom);

                $('button.delete', dom).button().click(function () {
                    that.deleteMedia($('img', $(this).parent().parent()).attr('src'));
                });
            });

            $('a[rel=nyro]', that.dialog).nyroModal();
        }
    });
};

ManageDocumentsAction.prototype.deleteMedia = function (source) {
    var that = this;
    Helpers.confirmDialog('Are you sure you want to delete this image?  This removes the file from the filesystem and is irreversible.', function (response) {
        if (response == 'yes') {
            $.ajax({
                url: site_base + 'admin/process/deletemedia/' + that.currentDocument.metadata.id,
                data: { reference: source },
                dataType: 'json',
                success: function (data) {
                    that.loadMedia(that.currentDocument.metadata.id);
                }
            });
        }
    });
};

ManageDocumentsAction.prototype.selectRedirect = function () {
    var that = this;
    var selected = false;

    var tree = $('<div></div>').jstree({
        'json_data': {
            data: global.documents
        },
        'plugins': ['themes', 'ui', 'json_data'],
        themes: { url: '/areas/admin/content/css/style-jstree.css' }
    }).bind('select_node.jstree', function (e, data) {
        selected = {
            id: data.rslt.obj.data('id'),
            name: data.rslt.obj.data('name')
        };
    });

    $('<div></div>').append(tree).dialog({
        title: 'Select a document',
        height: 600,
        modal: true,
        draggable: false,
        resizable: false,
        buttons: {
            'OK': function () {
                var dialog = this;
                if (selected) {
                    Helpers.confirmDialog('Are you sure you want to set this document to redirect to "' + selected.name + '"?', function (response) {
                        if (response == 'yes') {
                            $.ajax({
                                url: site_base + 'admin/process/setredirect/' + that.currentDocument.metadata.id,
                                data: { parent: selected.id },
                                type: 'POST',
                                success: function (data) {
                                    $(dialog).dialog('close');
                                    $(dialog).remove();

                                    $('input[name=redirect]', that.dialog).val(selected.name).attr('data-id', selected.id);
                                },
                                error: function () {
                                    Helpers.alertDialog('Something went wrong.');
                                }
                            });
                        }
                    });
                }
            },
            'Cancel': function () {
                $(this).dialog('close');
                $(this).remove();
            }
        }
    });
};

ManageDocumentsAction.prototype.reloadData = function () {
    var that = this;

    $.ajax({
        url: site_base + 'admin/json/getdocuments',
        success: function (data) {
            global.documents = data;

            var parent = $('.tree', that.dialog).parent();
            $('.tree', that.dialog).remove();

            parent.append('<div class="tree"></div>');

            $('.tree', that.dialog).jstree({
                selected: 0,
                plugins: ['themes', 'json_data', 'ui', 'crrm', 'contextmenu', 'dnd', 'types'],
                json_data: {
                    data: global.documents
                },
                types: {
                    file: {
                        icon: { image: site_base + 'admin/content/img/jstree/file.png' }
                    }
                },
                core: { animation: 0 },
                ui: { select_limit: 1 },
                themes: { url: '/areas/admin/content/css/style-jstree.css' }
            })
            .bind("create.jstree", function (e, data) { that.createDocument(e, data); })
	        .bind("remove.jstree", function (e, data) { that.deleteDocument(e, data); })
            .bind("rename.jstree", function (e, data) { that.renameDocument(e, data); })
            .bind("move_node.jstree", function (e, data) { that.moveDocument(e, data); })
            .bind("select_node.jstree", function (e, data) { that.loadDocument(e, data); });
        }
    });
};

ManageDocumentsAction.prototype.formatContent = function () {
    var input = $('textarea[name=content]').val();

    var output = style_html(input, {
        'indent_size': 2,
        'indent_char': ' ',
        'max_char': 80,
        'brace_style': 'collapse'
    });

    $('textarea[name=content]').val(output);
};

ManageDocumentsAction.prototype.previewContent = function () {
    $('<div class="preview"></div>').html($('textarea[name=content]').val()).dialog({
        autoOpen: true,
        title: 'Preview Document',
        height: 600,
        width: 800,
        close: function () { $(this).remove(); }
    });
};