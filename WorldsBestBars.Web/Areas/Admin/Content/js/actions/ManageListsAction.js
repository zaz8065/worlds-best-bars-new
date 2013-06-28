var ManageListsAction = function () {
    var that = this;
    this.dialog = false;
    this.data = false;
    this.currentList = false;
};

ManageListsAction.prototype.init = function () {
    var that = this;

    var initDialog = function () {
        $.ajax({
            url: site_base + 'admin/dialogs/managelists'
        }).success(function (data) {
            that.dialog = $(data);
            that.dialog.dialog({
                title: 'Manage lists',
                autoOpen: false,
                width: 800,
                height: 600,
                resizable: false,
                show: 'fade',
                hide: 'fade',
                create: function (event, ui) {
                    var rand = parseInt(Math.random() * 1000);

                    $('ul li.tab-head', that.dialog).each(function (index, el) {
                        var target = $('a', el).attr('href').substr(1);

                        $('.' + target, that.dialog).attr('id', rand + target);
                        $('a', el).attr('href', '#' + rand + target);
                    });

                    $('.right', that.dialog).tabs();

                    $('input, textarea', that.dialog).each(function (index, elem) {
                        if ($(elem).attr('id') != '') {
                            $(elem).attr('id', rand + '-' + $(elem).attr('id'));
                        }
                    });
                    $('label', that.dialog).each(function (index, elem) {
                        if ($(elem).attr('for') != '') {
                            $(elem).attr('for', rand + '-' + $(elem).attr('for'));
                        }
                    });

                    $('.left .create', that.dialog).button().click(function (e) {
                        e.preventDefault();
                        that.createList();
                    });

                    $('.manage-lists-right-search input[name=search]', that.dialog).keydown(function (e) {
                        if (e.keyCode == 13) {
                            e.preventDefault();
                            that.search($(this).val());
                        }
                    }).keyup(function (e) {
                        if (e.keyCode != 13) {
                            var theother = this;
                            clearTimeout($(this).data('search-timer'));
                            $(this).data('search-timer', setTimeout(function () { that.search($(theother).val()); }, 500));
                        }
                    });

                    $('.related-2 .existing', that.dialog).jScrollPane({ autoReinitialise: true });

                    $('button.save', that.dialog).button().unbind('click').click(function (e) { e.preventDefault(); that.saveList(); });

                    //$('.left .list', that.dialog).jScrollPane({ autoReinitialise: true })

                    $('.related-2 input[name=search]', that.dialog).keyup(function (e) {
                        var theother = this;
                        clearTimeout($(this).data('search-timer'));
                        $(this).data('search-timer', setTimeout(function () { that.relatedSearch($(theother).val()); }, 500));
                    });

                    that.reloadData();
                }
            });
        });
    };

    initDialog();
};

ManageListsAction.prototype.execute = function () {
    $(this.dialog).dialog('open');
};

ManageListsAction.prototype.reloadData = function (selected) {
    var that = this;

    $.ajax({
        url: site_base + 'admin/json/getlists',
        success: function (data) {
            that.data = data;

            var container = $('.list', that.dialog);
            container.empty();
            $(data).each(function (index, list) {
                var elem = $('<div data-id="' + list.id + '">' + list.name + '</div>');
                elem.appendTo(container);

                elem.click(function (e) { e.preventDefault(); $('[data-id]', $(this).parent()).removeClass('selected'); $(this).addClass('selected'); that.loadList($(this).attr('data-id')); });
            });

            if (selected) {
                $('div[data-id="' + selected + '"]', container).click();
            }
        }
    });
};

ManageListsAction.prototype.search = function (q) {
    var that = this;
    var container = $('.manage-lists-right-search .add .search-results', that.dialog);

    if (q == '') {
        container.empty();
    }

    $.ajax({
        url: site_base + 'admin/json/search',
        data: { search: q, limit: 16 },
        success: function (data) {

            container.empty();
            $(data).each(function (index, element) {
                var elem = $('<div class="result" data-id="' + element.id + '" data-type="' + element.type + '"><span class="name">' + element.name + '</span><span class="type">' + element.type + '</span></div>');
                container.append(elem);

                elem.click(function (e) { e.preventDefault(); that.addItem($(this).attr('data-id'), $(this).attr('data-type'), this); });
            });
        }
    });
};

ManageListsAction.prototype.loadList = function (id) {
    var that = this;
    var the_list = false;
    this.currentList = id;
    $(that.data).each(function (index, list) {
        if (list.id == id) {
            the_list = list;
            return false;
        }
    });

    that.loadRelated(id);

    var container = $('.manage-lists-right-search .existing .search-results', that.dialog);
    container.empty();

    if (the_list) {
        $('.left .list div[data-id]', that.dialog).removeClass('selected');
        $('.left .list div[data-id="' + id + '"]', that.dialog).addClass('selected');

        $('input[name=name]', that.dialog).val(the_list.name);
        $('input[name=key]', that.dialog).val(the_list.key);
        $('input[name=group]', that.dialog).val(the_list.group);
        $('input[name=searchable]', that.dialog).prop('checked', the_list.searchable);

        $(the_list.bars).each(function (index, item) {
            var elem = $('<div class="result" data-id="' + item.id + '" data-type="Bar"><span class="name">' + item.name + '</span><span class="type">Bar</span></div>');
            container.append(elem);

            elem.click(function (e) { e.preventDefault(); that.removeItem($(this).attr('data-id'), $(this).attr('data-type'), $(this)); });
        });
        $(the_list.locations).each(function (index, item) {
            var elem = $('<div class="result" data-id="' + item.id + '" data-type="Location"><span class="name">' + item.name + '</span><span class="type">Location</span></div>');
            container.append(elem);

            elem.click(function (e) { e.preventDefault(); that.removeItem($(this).attr('data-id'), $(this).attr('data-type'), $(this)); });
        });
        $(the_list.documents).each(function (index, item) {
            var elem = $('<div class="result" data-id="' + item.id + '" data-type="Document"><span class="name">' + item.name + '</span><span class="type">Document</span></div>');
            container.append(elem);

            elem.click(function (e) { e.preventDefault(); that.removeItem($(this).attr('data-id'), $(this).attr('data-type'), $(this)); });
        });
    }
};

ManageListsAction.prototype.addItem = function (id, type, elem) {
    var that = this;
    if (that.loading) { return; }
    if ($(elem).parent().parent().is('.existing')) {
        that.removeItem(id, type, elem);
        return;
    }

    that.loading = true;
    $.ajax({
        url: site_base + 'admin/process/AddToList/' + this.currentList,
        data: { item: id, type: type },
        success: function (data) {
            if (data != 'false') {
                $(elem).detach().appendTo($('.related .existing .search-results', that.dialog));
                // HACK - THIS IS NOT WORKING FOR SOME REASON.
                $(elem).unbind('click');
                $(elem).click(function (e) { e.preventDefault(); that.removeItem($(this).attr('data-id'), $(this).attr('data-type'), $(this)); });

                that.reloadData(id);
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

ManageListsAction.prototype.removeItem = function (id, type, elem) {
    var that = this;
    if (that.loading) { return; }
    that.loading = true;
    $.ajax({
        url: site_base + 'admin/process/RemoveFromList/' + this.currentList,
        data: { item: id, type: type },
        success: function (data) {
            if (data != 'false') {
                $(elem).remove();

                that.reloadData(id);
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

ManageListsAction.prototype.createList = function () {
    var that = this;
    if (that.loading) { return; }
    that.loading = true;

    Helpers.inputDialog('Name of new list:', function (response, value) {
        if (response) {
            $.ajax({
                url: site_base + 'admin/process/CreateList',
                data: { name: value },
                success: function (data) {
                    if (data != '') {
                        var container = $('.list', that.dialog);
                        var elem = $('<div data-id="' + data.id + '">' + data.name + '</div>');
                        elem.appendTo(container);
                        that.data.push(data);
                        elem.click(function (e) { e.preventDefault(); that.loadList($(this).attr('data-id')); });
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
        }
    });
};

ManageListsAction.prototype.loadRelated = function (id) {
    var that = this;
    if (!id) { return; }
    $.ajax({
        url: site_base + 'admin/json/getrelated/' + id,
        success: function (data) {
            var container = $('.related-2 .existing .search-results', that.dialog);

            container.empty();

            $(data).each(function (index, element) {
                var result = $('<div class="result" data-id="' + element.id + '"><span class="name">' + element.name + '</span><span class="type">' + element.type + '</span></div>');
                container.append(result);

                $(result).click(function (e) { e.preventDefault(); that.removeRelated($(this).attr('data-id'), this); });
            });
        }
    });
};

ManageListsAction.prototype.relatedSearch = function (query) {
    var that = this;
    $.ajax({
        url: site_base + 'admin/json/search',
        data: { search: query, limit: 15 },
        success: function (data) {
            var container = $('.related-2 .add .search-results', that.dialog);

            container.empty();

            $(data).each(function (index, element) {
                var result = $('<div class="result" data-id="' + element.id + '"><span class="name">' + element.name + '</span><span class="type">' + element.type + '</span></div>');
                container.append(result);

                $(result).click(function (e) { e.preventDefault(); that.addRelated($(this).attr('data-id'), this); });
            });
        }
    });
};

ManageListsAction.prototype.addRelated = function (id, elem) {
    var that = this;
    if (that.loading) { return; }
    that.loading = true;
    $.ajax({
        url: site_base + 'admin/process/AddRelated/' + this.currentList,
        data: { right: id },
        success: function (data) {
            if (data != 'false') {
                $(elem).detach().appendTo($('.related-2 .existing .search-results', that.dialog));
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

ManageListsAction.prototype.removeRelated = function (id, elem) {
    var that = this;
    if (that.loading) { return; }
    that.loading = true;
    $.ajax({
        url: site_base + 'admin/process/RemoveRelated/' + this.currentList,
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

ManageListsAction.prototype.saveList = function () {
    var that = this;

    $.ajax({
        url: site_base + 'admin/process/EditList/' + that.currentList,
        data: {
            name: $('input[name=name]', that.dialog).val(),
            key: $('input[name=key]', that.dialog).val(),
            group: $('input[name=group]', that.dialog).val(),
            searchable: $('input[name=searchable]', that.dialog).prop('checked')
        },
        success: function (data) {
            if (data != 'false') {
                that.reloadData(that.currentList);
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