var ManageAdvertsAction = function () {
    var that = this;
    this.dialog = false;
    this.dialogDom = false;
    this.data = false;
    this.currentAdvert = false;
    this.saveButtonDom = false;
    this.activateButtonDom = false;
    this.deleteButtonDom = false;
    this.viewButtonDom = false;
    this.plot = false;

    this.getAdvert = function (id) {
        if (!this.data) { return false; }
        var result = false;
        $(this.data).each(function (index, advert) {
            if (advert.id == id) {
                result = advert;
                return false;
            }
        });
        return result;
    };
};

ManageAdvertsAction.prototype.init = function () {
    var that = this;

    var initDialog = function () {
        $.ajax({
            url: site_base + 'admin/dialogs/manageadverts'
        }).success(function (data) {
            that.dialog = $(data).dialog({
                title: 'Manage adverts',
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

                    that.saveButtonDom.button({ disabled: true }).click(function (e) { e.preventDefault(); that.saveAdvert(); });
                    that.activateButtonDom.button({ disabled: true }).click(function (e) { e.preventDefault(); that.activateAdvert(); });
                    that.viewButtonDom.button({ disabled: true }).click(function (e) { e.preventDefault(); that.viewAdvert(); });
                    that.createButtonDom.button().click(function (e) { e.preventDefault(); that.createAdvert(); });
                    that.deleteButtonDom.button({ disabled: true }).click(function (e) { e.preventDefault(); that.deleteAdvert(); });

                    $('input[type=date]', that.dialogDom).datepicker({dateFormat: 'yy-M-dd'});

                    $('select', that.dialogDom).selectmenu();

                    $('.graph-container').empty().append('<div id="graph-' + rand + '"></div>');

                    //that.plot = $.jqplot('graph-' + rand, [], { height: 488, width: 490 });

                    that.reloadData();
                }
            });
        });
    };

    initDialog();
};

ManageAdvertsAction.prototype.execute = function () {
    $(this.dialog).dialog('open');
};

ManageAdvertsAction.prototype.reloadData = function (selected) {
    var that = this;

    $.ajax({
        url: site_base + 'admin/json/getadverts',
        dataType: 'json'
    }).success(function (data) {
        that.data = data; 

        var container = $('.list', that.dialogDom);

        container.empty();

        $(data).each(function (index, advert) {
            var advertDom = $('<div class="advert" data-id="' + advert.id + '"><span>' + advert.name + '</span></div>');

            $(advertDom).click(function (e) { e.preventDefault(); $('[data-id]', $(this).parent()).removeClass('selected'); $(this).addClass('selected');that.showAdvert($(this).attr('data-id')); });

            if (!advert.active) {
                $('span', advertDom).addClass('inactive');
            }

            container.append(advertDom);
        });

        if (selected) {
            $('a[data-id="' + selected + '"]', container).click();
        }
        else {
            $('a[data-id]:first', container).click();
        }
    });
};

ManageAdvertsAction.prototype.showAdvert = function (id) {
    var that = this;
    var advert = this.getAdvert(id);

    this.currentAdvert = advert;

    $('input[name=title]', this.dialogDom).val(advert.name);
    $('textarea[name=body]', this.dialogDom).val(advert.body);
    $('input[name=skyscraper]', this.dialogDom).val(advert.skyscraper);
    $('input[name=video]', this.dialogDom).val(advert.video);
    $('input[name=start]', this.dialogDom).datepicker('setDate', advert.start == null ? null : new Date(advert.start));
    $('input[name=finish]', this.dialogDom).datepicker('setDate', advert.finish == null ? null : new Date(advert.finish));
    $('input[name=weight]', this.dialogDom).val(advert.weight);
    $('input[name=url]', this.dialogDom).val(advert.destination);
    $('select[name=type]', this.dialogDom).val(advert.type);

    that.activateButtonDom.button('option', 'label', advert.active ? 'Deactivate' : 'Activate');
    that.activateButtonDom.button('option', 'disabled', false);
    that.saveButtonDom.button('option', 'disabled', false);
    that.deleteButtonDom.button('option', 'disabled', false);

    that.loadStats(id);
};

ManageAdvertsAction.prototype.saveAdvert = function () {
    var that = this;

    var submit_data = {
        'title': $('input[name=title]', this.dialogDom).val(),
        'body': $('textarea[name=body]', this.dialogDom).val(),
        'start': $('input[name=start]', this.dialogDom).val(),
        'finish': $('input[name=finish]', this.dialogDom).val(),
        'skyscraper': $('input[name=skyscraper]', this.dialogDom).val(),
        'video': $('input[name=video]', this.dialogDom).val(),
        'start': $('input[name=start]', this.dialogDom).datepicker('getDate') == null ? '' : $.datepicker.formatDate('yy-mm-dd', $('input[name=start]', this.dialogDom).datepicker('getDate')),
        'finish': $('input[name=finish]', this.dialogDom).datepicker('getDate') == null ? '' : $.datepicker.formatDate('yy-mm-dd', $('input[name=finish]', this.dialogDom).datepicker('getDate')),
        'weight': $('input[name=weight]', this.dialogDom).val(),
        'type': $('select[name=type]', this.dialogDom).val(),
        'destination': $('input[name=url]', this.dialogDom).val()
    };

    $.ajax({
        url: site_base + 'admin/process/editadvert/' + this.currentAdvert.id,
        data: submit_data,
        type: 'POST'
    }).success(function (data) {
        if (data == 'false') {
            Helpers.alertDialog('Something went wrong.');
        }
        else {
            that.reloadData(that.currentAdvert.id);
        }
    });
};

ManageAdvertsAction.prototype.activateAdvert = function () {
    var that = this;

    Helpers.confirmDialog('Are you sure you want to ' + (that.currentAdvert.active ? 'de' : '') + 'activate this advert?', function (response) {
        if (response == 'yes') {
            var submit_data = {
                active: !that.currentAdvert.active
            };

            $.ajax({
                url: site_base + 'admin/process/toggleadvertactivation/' + that.currentAdvert.id,
                data: submit_data,
                type: 'POST'
            }).success(function (data) {
                if (data != 'true') {
                    Helpers.alertDialog('Something went wrong.');
                }
                else {
                    that.reloadData(that.currentAdvert.id);
                }
            });
        }
    });
};

ManageAdvertsAction.prototype.viewAdvert = function () {
    var that = this;

    Helpers.alertDialog('Not yet implemented.');
};

ManageAdvertsAction.prototype.createAdvert = function () {
    var that = this;

    Helpers.inputDialog('Title of the new advert:', function (valid, text) {
        var submit_data = {
            title: text
        };

        $.ajax({
            url: site_base + 'admin/process/createadvert',
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

ManageAdvertsAction.prototype.deleteAdvert = function () {
    var that = this;

    Helpers.confirmDialog('Are you sure you want to delete this advert?', function (response) {
        if (response == 'yes') {
            Helpers.confirmDialog('Are you REALLY sure you want to delete this advert?', function (response2) {
                if (response2 == 'yes') {
                    $.ajax({
                        url: site_base + 'admin/process/deleteadvert/' + that.currentAdvert.id
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

ManageAdvertsAction.prototype.loadStats = function (id) {
    var that = this;
    $('.stats', that.dialog).empty();

    $.ajax({
        url: site_base + 'admin/json/getadvertstats/' + id,
        success: function (data) {
            var table = $('<table></table>');
            table.append('<thead><tr><th>Period</th><th class="num">Impressions</th><th class="num">Clicks</th></tr></thead>');

            var body = $('<tbody></tbody>');
            table.append(body);

            $(data).each(function (index, stat) {
                var period = new Date(stat.period);
                body.append('<tr><td>' + period.toDateString() + ' ' + period.getHours() + ':00</td><td class="num">' + stat.impressions + '</td><td class="num">' + stat.clicks + '</td></tr>');
            });

            $('.stats', that.dialog).append(table);
        }
    });
};