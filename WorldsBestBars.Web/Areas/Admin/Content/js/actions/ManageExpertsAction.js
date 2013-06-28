var ManageExpertsAction = function () {
    var that = this;
    this.dialog = false;
    this.dialogDom = false;
    this.data = false;
    this.currentExpert = false;
    this.saveButtonDom = false;
    this.activateButtonDom = false;
    this.deleteButtonDom = false;
    this.viewButtonDom = false;
    this.loading = false;

    this.getExpert = function (id) {
        if (!this.data) { return false; }
        var result = false;
        $(this.data).each(function (index, expert) {
            if (expert.id == id) {
                result = expert;
                return false;
            }
        });
        return result;
    };
};

ManageExpertsAction.prototype.init = function () {
    var that = this;

    var initDialog = function () {
        $.ajax({
            url: site_base + 'admin/dialogs/manageexperts'
        }).success(function (data) {
            that.dialog = $(data).dialog({
                title: 'Manage experts',
                autoOpen: false,
                width: 800,
                height: 600,
                resizable: false,
                show: 'fade',
                hide: 'fade',
                create: function (event, ui) {
                    that.dialogDom = event.target;
                    var rand = parseInt(Math.random() * 1000);

                    $('ul li.tab-head', that.dialogDom).each(function (index, el) {
                        var target = $('a', el).attr('href').substr(1);

                        $('.' + target, that.dialogDom).attr('id', rand + target);
                        $('a', el).attr('href', '#' + rand + target);
                    });

                    $('.right', that.dialogDom).tabs();


                    $('input, textarea', that.dialogDom).each(function (index, elem) {
                        if ($(elem).attr('id') != '') {
                            $(elem).attr('id', rand + '-' + $(elem).attr('id'));
                        }
                    });
                    $('label', that.dialogDom).each(function (index, elem) {
                        if ($(elem).attr('for') != '') {
                            $(elem).attr('for', rand + '-' + $(elem).attr('for'));
                        }
                    });

                    $('.scrollable', that.dialogDom).jScrollPane({ autoReinitialise: true });

                    that.saveButtonDom = $('button.save', that.dialogDom);
                    that.activateButtonDom = $('button.activate', that.dialogDom);
                    that.viewButtonDom = $('button.view', that.dialogDom);
                    that.createButtonDom = $('button.create', that.dialogDom);
                    that.deleteButtonDom = $('button.delete', that.dialogDom);
                    that.createReviewButtonDom = $('.add-review button', that.dialogDom);

                    that.saveButtonDom.button({ disabled: true }).click(function (e) { e.preventDefault(); that.saveExpert(); });
                    that.activateButtonDom.button({ disabled: true }).click(function (e) { e.preventDefault(); that.activateExpert(); });
                    that.viewButtonDom.button({ disabled: true }).click(function (e) { e.preventDefault(); that.viewExpert(); });
                    that.createButtonDom.button().click(function (e) { e.preventDefault(); that.createExpert(); });
                    that.deleteButtonDom.button({ disabled: true }).click(function (e) { e.preventDefault(); that.deleteExpert(); });
                    that.createReviewButtonDom.button({ disabled: true }).click(function (e) { e.preventDefault(); that.createReview(); });

                    $('.add-review input[name=bar]', that.dialogDom).autocomplete({
                        source: site_base + 'admin/json/bars',
                        select: function (event, ui) {
                            event.preventDefault();

                            $(this).val(ui.item.label);
                            $(this).attr('selected-id', ui.item.value);
                        },
                        focus: function (event, ui) {
                            event.preventDefault();
                        },
                        search: function (event, ui) {
                            $(this).attr('selected-id', '');
                        }
                    });

                    $('.review_container').jScrollPane({ autoReinitialise: true });
                    $('.manage-experts-right-related .existing', that.dialogDom).jScrollPane({ autoReinitialise: true });

                    $('.manage-experts-right-related input[name=search]', that.dialogDom).keyup(function (e) {
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

ManageExpertsAction.prototype.execute = function () {
    $(this.dialog).dialog('open');
};

ManageExpertsAction.prototype.reloadData = function (selected) {
    var that = this;

    $.ajax({
        url: site_base + 'admin/json/getexperts'
    }).success(function (data) {
        that.data = data;

        var container = $('.list', that.dialogDom);
        container.empty();

        $(data).each(function (index, expert) {
            var expertDom = $('<div class="expert" data-id="' + expert.id + '"><span>' + expert.name + '</span></div>');

            $(expertDom).click(function (e) { e.preventDefault(); $('[data-id]', $(this).parent()).removeClass('selected'); $(this).addClass('selected'); that.showExpert($(this).attr('data-id')); });

            if (!expert.active) {
                $('span', expertDom).addClass('inactive');
            }

            container.append(expertDom);
        });

        if (selected) {
            $('a[data-id="' + selected + '"]', container).click();
        }
        else {
            $('a[data-id]:first', container).click();
        }
    });
};

ManageExpertsAction.prototype.showExpert = function (id) {
    var that = this;
    var expert = this.getExpert(id);

    that.loadRelated(id);
    that.loadMedia(id);

    $('.upload-form', that.dialog).attr('action', '/admin/process/uploadmedia/' + id);
    $('.upload-form', that.dialog).bind('submit', function () {
        return AIM.submit(this, {
            'onStart': function () {
            },
            'onComplete': function () {
                that.loadMedia(id);
            }
        });
    });

    this.currentExpert = expert;

    $('input[name=name]', this.dialogDom).val(expert.name);
    $('input[name=title]', this.dialogDom).val(expert.title);
    $('textarea[name=biography]', this.dialogDom).val(expert.biography);
    $('input[name=urlkey]', this.dialogDom).val(expert.urlkey);
    $('input[name=website]', this.dialogDom).val(expert.website);

    var reviewContainer = $('.reviews', this.dialogDom);

    reviewContainer.empty();

    $.ajax({
        url: site_base + 'admin/json/GetUserReviewsForExpert/' + id,
        success: function (data) {
            reviewContainer.empty();

            $(data).each(function (index, review) {
                reviewContainer.append($('<div class="review clearfix" data-id="' + review.id + '"><span class="bar" data-id="' + review.bar.id + '">' + review.bar.name + '</span><div class="text">' + review.comment + '</div><div class="actions"><a href="#" class="delete">Delete</a></div></div>'));
            });

            $('a.delete', reviewContainer).button().click(function (e) {
                e.preventDefault();
                var id = $(this).parent().parent().attr('data-id');
                Helpers.confirmDialog('Are you sure you want to delete this review?', function (response) {
                    if (response == 'yes') {
                        $.ajax({
                            url: site_base + 'admin/process/deleteexpertreview/' + id,
                            type: 'POST',
                            success: function (data) {
                                if (data == 'false') {
                                    Helpers.alertDialog('Something went wrong.');
                                }
                                else {
                                    that.reloadData(that.currentExpert.id);
                                }
                            }
                        });
                    }
                });
            });
        }
    });

    that.activateButtonDom.button('option', 'label', expert.active ? 'Deactivate' : 'Activate');
    that.viewButtonDom.button('option', 'disabled', false);
    that.activateButtonDom.button('option', 'disabled', false);
    that.saveButtonDom.button('option', 'disabled', false);
    that.deleteButtonDom.button('option', 'disabled', false);
    that.createReviewButtonDom.button('option', 'disabled', false);

    $('.add-review input[name=bar]', this.dialogDom).val('');
    $('.add-review textarea[name=review]', this.dialogDom).val('');
};

ManageExpertsAction.prototype.saveExpert = function () {
    var that = this;

    var submit_data = {
        'name': $('input[name=name]', this.dialogDom).val(),
        'title': $('input[name=title]', this.dialogDom).val(),
        'biography': $('textarea[name=biography]', this.dialogDom).val(),
        'urlkey': $('input[name=urlkey]', this.dialogDom).val(),
        'website': $('input[name=website]', this.dialogDom).val()
    };

    $.ajax({
        url: site_base + 'admin/process/editexpert/' + this.currentExpert.id,
        data: submit_data,
        type: 'POST'
    }).success(function (data) {
        if (data == 'false') {
            Helpers.alertDialog('Something went wrong.');
        }
        else {
            that.reloadData(that.currentExpert.id);
        }
    });
};

ManageExpertsAction.prototype.activateExpert = function () {
    var that = this;

    Helpers.confirmDialog('Are you sure you want to ' + (that.currentExpert.active.active ? 'de' : '') + 'activate this expert?', function (response) {
        if (response == 'yes') {

            var submit_data = {
                active: !that.currentExpert.active
            };

            $.ajax({
                url: site_base + 'admin/process/toggleexpertactivation/' + that.currentExpert.id,
                data: submit_data,
                type: 'POST'
            }).success(function (data) {
                if (data != 'true') {
                    Helpers.alertDialog('Something went wrong.');
                }
                else {
                    that.reloadData(that.currentExpert.id);
                }
            });
        }
    });
};

ManageExpertsAction.prototype.viewExpert = function () {
    var that = this;

    Helpers.alertDialog('Not yet implemented.');
};

ManageExpertsAction.prototype.createExpert = function () {
    var that = this;

    Helpers.inputDialog('Name of the new expert:', function (valid, text) {
        var submit_data = {
            name: text
        };

        $.ajax({
            url: site_base + 'admin/process/createexpert',
            data: submit_data,
            type: 'POST'
        }).success(function (data) {
            if (data == 'false') {
                Helpers.alertDialog('Something went wrong.');
            }
            else {
                that.reloadData(data);
            }
        });
    });
};

ManageExpertsAction.prototype.deleteExpert = function () {
    var that = this;

    Helpers.confirmDialog('Are you sure you want to delete this expert?', function (response) {
        if (response == 'yes') {
            Helpers.confirmDialog('Are you REALLY sure you want to delete this expert?  Doing so will remove all reviews and media items associated with this expert.', function (response2) {
                if (response2 == 'yes') {

                    $.ajax({
                        url: site_base + 'admin/process/deleteexpert/' + that.currentExpert.id
                    }).success(function (data) {
                        if (data == 'false') {
                            Helpers.alertDialog('Something went wrong.');
                        }
                        else {
                            that.reloadData();
                        }
                    });
                }
            });
        }
    });
};

ManageExpertsAction.prototype.createReview = function () {
    var that = this;

    var submit_data = {
        bar: $('.add-review input[name=bar]').attr('selected-id'),
        expert: that.currentExpert.id,
        comment: $('.add-review textarea[name=review]').val()
    };

    $.ajax({
        url: site_base + 'admin/process/createexpertreview',
        data: submit_data,
        type: 'POST'
    }).success(function (data) {
        if (data == 'false') {
            Helpers.alertDialog('Something went wrong.');
        }
        else {
            that.showExpert(that.currentExpert.id);
        }
    });
};

ManageExpertsAction.prototype.loadRelated = function (id) {
    var that = this;
    if (!id) { return; }
    $.ajax({
        url: site_base + 'admin/json/getrelated/' + id,
        success: function (data) {
            var container = $('.manage-experts-right-related .existing .search-results', that.dialogDom);

            container.empty();

            $(data).each(function (index, element) {
                var result = $('<div class="result" data-id="' + element.id + '"><span class="name">' + element.name + '</span><span class="type">' + element.type + '</span></div>');
                container.append(result);

                $(result).click(function (e) { e.preventDefault(); that.removeRelated($(this).attr('data-id'), this); });
            });
        }
    });
};

ManageExpertsAction.prototype.relatedSearch = function (query) {
    var that = this;
    $.ajax({
        url: site_base + 'admin/json/search',
        data: { search: query, limit: 15 },
        success: function (data) {
            var container = $('.manage-experts-right-related .add .search-results', that.dialogDom);

            container.empty();

            $(data).each(function (index, element) {
                var result = $('<div class="result" data-id="' + element.id + '"><span class="name">' + element.name + '</span><span class="type">' + element.type + '</span></div>');
                container.append(result);

                $(result).click(function (e) { e.preventDefault(); that.addRelated($(this).attr('data-id'), this); });
            });
        }
    });
};

ManageExpertsAction.prototype.addRelated = function (id, elem) {
    var that = this;
    if (that.loading) { return; }
    that.loading = true;
    $.ajax({
        url: site_base + 'admin/process/AddRelated/' + this.currentExpert.id,
        data: { right: id },
        success: function (data) {
            if (data != 'false') {
                $(elem).detach().appendTo($('.manage-experts-right-related .existing .search-results', that.dialogDom));
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

ManageExpertsAction.prototype.removeRelated = function (id, elem) {
    var that = this;
    if (that.loading) { return; }
    that.loading = true;
    $.ajax({
        url: site_base + 'admin/process/RemoveRelated/' + this.currentExpert.id,
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

ManageExpertsAction.prototype.loadMedia = function (id) {
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

ManageExpertsAction.prototype.deleteMedia = function (source) {
    var that = this;
    Helpers.confirmDialog('Are you sure you want to delete this image?  This removes the file from the filesystem and is irreversible.', function (response) {
        if (response == 'yes') {
            $.ajax({
                url: site_base + 'admin/process/deletemedia/' + that.currentExpert.id,
                data: { reference: source },
                dataType: 'json',
                success: function (data) {
                    that.loadMedia(that.currentExpert.id);
                }
            });
        }
    });
};