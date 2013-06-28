var CreateBarAction = function () {
    this.dialogHtml = '';
};

CreateBarAction.prototype.init = function () {
    var _instance = this;

    var getDialogHtml = function () {
        $.ajax({
            url: site_base + 'admin/dialogs/editbar',
            async: false
        }).success(function (data) {
            _instance.dialogHtml = data;
        });
    };

    getDialogHtml();
};

CreateBarAction.prototype.execute = function (source) {
    new CreateBarExecution(this.dialogHtml, $(source).attr('data-id'));
};

var CreateBarExecution = function (html, id) {
    var instance = this;
    var that = this;

    instance.clearMarkerHints = function () {
        if (instance.map_markerhints && instance.map_markerhints.length > 0) {
            $(instance.map_markerhints).each(function (index, element) {
                element.setMap(null);
            });
        }

        instance.map_markerhints = [];
    };

    instance.moveMapMarker = function (latlng) {
        instance.clearMarkerHints();
        instance.map_latlng = latlng;
        if (!instance.map_marker) {
            instance.map_marker = new google.maps.Marker({
                position: instance.map_latlng,
                map: instance.map,
                draggable: true
            });

            google.maps.event.addListener(instance.map_marker, 'dragend', function () {
                instance.clearMarkerHints();
                instance.map_latlng = instance.map_marker.getPosition();
            });
        }
        else {
            instance.map_marker.setPosition(instance.map_latlng);
        }
    };

    instance.bestGuessMapLocation = function (search) {
        var request = { 'address': search };
        instance.geocoder.geocode(request, function (results, status) {
            if (results.length > 0) {
                var bounds = new google.maps.LatLngBounds();

                instance.clearMarkerHints();

                instance.map_markerhints = [];
                $(results).each(function (index, element) {
                    var latlng = element.geometry.location;
                    bounds.extend(latlng);

                    var _marker = new google.maps.Marker({
                        position: latlng,
                        draggable: false,
                        map: instance.map,
                        flat: true
                    });

                    google.maps.event.addListener(_marker, 'click', function (event) {
                        instance.moveMapMarker(this.getPosition());
                    });

                    instance.map_markerhints.push(_marker);
                });

                instance.map.fitBounds(bounds);
            }
            else {
                Helpers.alertDialog('Nothing was found for the specified address');
            }
        });
    };

    instance.fixMapResize = function () {
        if (!instance.map_resized) {
            instance.map_resized = true;
            google.maps.event.trigger(instance.map, 'resize');
            instance.map.setCenter(instance.map_latlng);
        }
    };

    this.dialog = $(html);

    this.dialog.dialog({
        title: id ? 'Create a new bar' : 'Edit bar',
        width: 800,
        height: 600,
        resizable: false,
        show: 'fade',
        hide: 'fade',
        create: function (event, ui) {
            var rand = parseInt(Math.random() * 1000);

            var tabContainer = $('.right', $(this));
            $('ul li', tabContainer).each(function (index, el) {
                var target = $('a', el).attr('href').substr(1);

                var panel = $('.' + target, tabContainer);

                $(panel).attr('id', rand + target);
                $('a', el).attr('href', '#' + rand + target);
            });

            // avoid id clashes
            $('input, textarea', this).each(function (index, elem) {
                if ($(elem).attr('id') != '') {
                    $(elem).attr('id', rand + '-' + $(elem).attr('id'));
                }
            });
            $('label', this).each(function (index, elem) {
                if ($(elem).attr('for') != '') {
                    $(elem).attr('for', rand + '-' + $(elem).attr('for'));
                }
            });

            tabContainer.tabs().bind('tabsshow', instance.fixMapResize);

            $('.scrollable', this).jScrollPane({
                autoReinitialise: true
            });

            $('.openhours input[type=text]', this).timepicker({
                minutes: { starts: 0, ends: 45, interval: 15 },
                // this logic below makes sure start times can't come after end times and vice versa, if it's not working correctly - drop it.
                onHourShow: function (hour) {
                    var index = $('input', $(this).parent()).index(this);
                    var from = index == 0 || index == 2;
                    if (from) {
                        var to = $('input', $(this).parent()).eq(index + 1);
                        var toHour = $(to).timepicker('getHour');
                        return toHour == -1 || toHour == 12 || toHour >= hour;
                    }
                    else {
                        from = $('input', $(this).parent()).eq(index - 1);
                        var fromHour = $(from).timepicker('getHour');
                        return fromHour == -1 || fromHour == 12 || fromHour <= hour;
                    }
                },
                onMinuteShow: function (hour, minute) {
                    var index = $('input', $(this).parent()).index(this);
                    var from = index == 0 || index == 2;
                    if (from) {
                        var to = $('input', $(this).parent()).eq(index + 1);
                        var toHour = $(to).timepicker('getHour');
                        var toMinute = $(to).timepicker('getMinute');
                        if (toHour == -1 || toHour == 12 || hour < toHour) { return true; }
                        if (hour == toHour && minute < toMinute) { return true; }
                        return false;
                    }
                    else {
                        from = $('input', $(this).parent()).eq(index - 1);
                        var fromHour = $(from).timepicker('getHour');
                        return fromHour <= hour;
                        if (fromHour == -1 || fromHour == 12 || hour > fromHour) { return true; }
                        if (hour == fromHour && minute > toMinute) { return true; }
                        return false;
                    }
                }
            });

            $('.find-on-map', this).button().click(function (e) {
                e.preventDefault();
                tabContainer.tabs('select', 0);
                var search = $('input[name=name]', this.dialog) + ' ';
                $('input', $(this).parent().parent()).each(function (index, element) {
                    search += $(element).val() + ' ';
                });

                instance.bestGuessMapLocation(search);
            });

            instance.geocoder = new google.maps.Geocoder();
            instance.map_latlng = new google.maps.LatLng(20, 0);
            instance.map = new google.maps.Map($('.map', this)[0], {
                zoom: 2,
                center: instance.map_latlng,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            });

            google.maps.event.addListener(instance.map, 'click', function (event) {
                instance.moveMapMarker(event.latLng);
            });

            $('.bar-activation button.activate', this).button({ disabled: true }).click(function (e) { e.preventDefault(); instance.toggleActivation(); });
            $('.bar-activation button.view', this).button({ disabled: true }).click(function (e) { e.preventDefault(); instance.viewBar(); });
            $('.bar-activation', this).detach().appendTo($('.ui-dialog-buttonpane', $(this).parent()));

            instance.selectLocationButton = $('.select-location button', this);
            instance.selectLocationButton.button({ disabled: true }).click(function (e) { e.preventDefault(); instance.selectLocation(); });

            $('.related .existing', that.dialog).jScrollPane({ autoReinitialise: true });

            $('.related input[name=search]', that.dialog).keyup(function (e) {
                var theother = this;
                clearTimeout($(this).data('search-timer'));
                $(this).data('search-timer', setTimeout(function () { that.relatedSearch($(theother).val()); }, 500));
            });

            if (id) {
                instance.loadBar(id);
            }
        },
        open: function (event, ui) {
            instance.fixMapResize();
        },
        beforeClose: function (event, ui) {
            $(this).dialog('destroy');
            $(this).remove();

            return false;
        },
        buttons: {
            'Save': function () {
                instance.saveBar();
            },
            'Close': function () {
                $(this).dialog('destroy');
                $(this).remove();
            }
        }
    });
};

CreateBarExecution.prototype.loadBar = function (id) {
    var that = this;
    var __ = that.dialog;

    that.bar_id = id;

    that.loadRelated(id);
    that.loadReviews(id);
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

    $.ajax({
        url: site_base + 'admin/json/getbar/' + id
    }).success(function (data) {
        that.bar_data = data;

        $('.select-location span:first', __).text(data.url);
        $('input[name=name]', __).val(data.name);
        $('textarea[name=intro]', __).val(data.intro);
        $('textarea[name=description]', __).val(data.description);
        $('input[name=urlkey]', __).val(data.urlkey);
        $('input[name=menu_url]', __).val(data.menu_url);
        if (data.address != null) {
            $('input[name=street1]', __).val(data.address.street1);
            $('input[name=street2]', __).val(data.address.street2);
            $('input[name=city]', __).val(data.address.city);
            $('input[name=county]', __).val(data.address.county);
            $('input[name=postcode]', __).val(data.address.postcode);
            $('input[name=country]', __).val(data.address.country);
        }
        $('input[name=phone]', __).val(data.phone);
        $('input[name=fax]', __).val(data.fax);
        $('input[name=email]', __).val(data.email);
        $('input[name=website]', __).val(data.website);
        if (data.openhours != null) {
            try {
                $('input[name=mon1]', __).val(data.openhours.monday.fromA);
                $('input[name=mon2]', __).val(data.openhours.monday.toA);
                $('input[name=mon3]', __).val(data.openhours.monday.fromB);
                $('input[name=mon4]', __).val(data.openhours.monday.toB);

                $('input[name=tue1]', __).val(data.openhours.tuesday.fromA);
                $('input[name=tue2]', __).val(data.openhours.tuesday.toA);
                $('input[name=tue3]', __).val(data.openhours.tuesday.fromB);
                $('input[name=tue4]', __).val(data.openhours.tuesday.toB);

                $('input[name=wed1]', __).val(data.openhours.wednesday.fromA);
                $('input[name=wed2]', __).val(data.openhours.wednesday.toA);
                $('input[name=wed3]', __).val(data.openhours.wednesday.fromB);
                $('input[name=wed4]', __).val(data.openhours.wednesday.toB);

                $('input[name=thurs1]', __).val(data.openhours.thursday.fromA);
                $('input[name=thurs2]', __).val(data.openhours.thursday.toA);
                $('input[name=thurs3]', __).val(data.openhours.thursday.fromB);
                $('input[name=thurs4]', __).val(data.openhours.thursday.toB);

                $('input[name=fri1]', __).val(data.openhours.friday.fromA);
                $('input[name=fri2]', __).val(data.openhours.friday.toA);
                $('input[name=fri3]', __).val(data.openhours.friday.fromB);
                $('input[name=fri4]', __).val(data.openhours.friday.toB);

                $('input[name=sat1]', __).val(data.openhours.saturday.fromA);
                $('input[name=sat2]', __).val(data.openhours.saturday.toA);
                $('input[name=sat3]', __).val(data.openhours.saturday.fromB);
                $('input[name=sat4]', __).val(data.openhours.saturday.toB);

                $('input[name=sun1]', __).val(data.openhours.sunday.fromA);
                $('input[name=sun2]', __).val(data.openhours.sunday.toA);
                $('input[name=sun3]', __).val(data.openhours.sunday.fromB);
                $('input[name=sun4]', __).val(data.openhours.sunday.toB);
            }
            catch (ex) { }
        }

        $('.features', __).empty();
        for (var i = 0; i < data.features.length; i++) {
            var data_feature = data.features[i];
            var feature = $('<div class="feature"></div>').attr('data-id', data_feature.id);
            feature.append('<label for="feature_name_' + i + '">Name</label>');
            feature.append('<input id="feature_name_' + i + '" class="name" value="' + data_feature.name + '" />');
            feature.append('<label for="feature_sponsor_' + i + '">Sponsor</label>');
            feature.append('<select id="feature_sponsor_' + i + '" class="sponsor" value="' + (data_feature.sponsor == null ? '' : data_feature.sponsor) + '"><option value=""/><option value="absolut">Absolut</option><option value="beefeater">Beefeater</option><option value="chivas">Chivas</option><option value="havana-club">Havana Club</option><option value="jameson">Jameson</option></select>');
            feature.append('<label for="feature_details_' + i + '">Description</label>');
            feature.append('<textarea id="feature_details_' + i + '" class="details">' + data_feature.details + '</textarea>');
            feature.append('<label for="feature_extra_' + i + '">Extra</label>');
            feature.append('<textarea id="feature_extra_' + i + '" class="extra">' + (data_feature.extra == null ? '' : data_feature.extra) + '</textarea>');
            feature.append('<label for="feature_type_' + i + '">Type</label>');
            feature.append('<select id="feature_type_' + i + '" class="type" value="' + data_feature.type + '"><option value="regular-feature">Regular feature</option><option value="signature-cocktail">Signature cocktail</option></select>');
            feature.append('<label for="feature_active_' + i + '">Is Active</label>');
            feature.append('<input id="feature_active_' + i + '" class="active" type="checkbox"' + (data_feature.active ? ' checked' : '') + ' />');

            var upload = $('<form id="feature_upload_' + i + '" enctype="multipart/form-data" method="POST"><input type="file" name="file"/><input type="submit" value="Upload" /></form>');
            feature.append(upload);

            feature.append('<input data-id="' + data_feature.id + '" id="feature_save_' + i + '" class="save" type="button" value="Save" />');
            feature.append('<input data-id="' + data_feature.id + '" id="feature_delete_' + i + '" class="delete" type="button" value="Delete" />');
            feature.append('<hr />');
            $('form', feature).attr('action', '/admin/process/featureuploadmedia/' + data_feature.id);
            $('form', feature).bind('submit', function () {
                return AIM.submit(this, {
                    'onStart': function () {
                    },
                    'onComplete': function () {
                        alert('Uploaded');
                    }
                });
            });

            $('.save', feature).click(function (e) {
                e.preventDefault();
                var _feature = $(this).parent();
                var data = {
                    name: $('.name', _feature).val(),
                    sponsor: $('.sponsor', _feature).val(),
                    details: $('.details', _feature).val(),
                    extra: $('.extra', _feature).val(),
                    type: $('.type', _feature).val(),
                    isActive: $('.active', _feature).is(':checked'),
                };

                $.post('/admin/process/editfeature/' + $(this).attr('data-id'), data, function () { that.loadBar(that.bar_id); });
            });

            $('.delete', feature).click(function (e) {
                e.preventDefault();
                $.post('/admin/process/deletefeature/' + $(this).attr('data-id'), function () { that.loadBar(that.bar_id); });
            });
            

            $('.features', __).append(feature);
        }

        $('.create-bar-right-features .save', __).button();
        $('.create-bar-right-features .delete', __).button();
        $('.create-bar-right-features .upload', __).button();
        $('.create-bar-right-features .add_feature .save', __).click(function () {
            var data = {
                name: $('.add_feature .name').val(),
                sponsor: $('.add_feature .sponsor').val(),
                details: $('.add_feature .details').val(),
                extra: $('.add_feature .extra').val(),
                type: $('.add_feature .type').val(),
                isActive: $('.add_feature .active').is(':checked'),
                bar: that.bar_id
            };

            $.post('/admin/process/createfeature', data, function () {
                that.loadBar(that.bar_id);

                $('.add_feature .name').val('');
                $('.add_feature .sponsor').val('');
                $('.add_feature .details').val('');
                $('.add_feature .extra').val('');
                $('.add_feature .type').val('');
            });
        });

        $('create-bar-right-features').click();
        /*$('.create-bar-right-features .menu-upload form', __).attr('action', '/admin/process/uploadmenu/' + that.bar_id);
        $('.create-bar-right-features .menu-upload form', feature).bind('submit', function () {
            return AIM.submit(this, {
                'onStart': function () {
                },
                'onComplete': function () {
                    alert('Uploaded');
                }
            });
        });*/


        if (data.geo != null) {
            var latlng = new google.maps.LatLng(data.geo.lat, data.geo.long);

            that.moveMapMarker(latlng);
            that.map.setCenter(latlng);
            that.map.setZoom(17);
        }

        $('.bar-activation button.activate', $(__).parent()).button('option', 'disabled', false);
        $('.bar-activation button.activate', $(__).parent()).button('option', 'label', (data.active ? 'Deactivate' : 'Activate'));
        $('.bar-activation button.view', $(__).parent()).button('option', 'disabled', !data.active);

        that.selectLocationButton.button('option', 'disabled', false);
        that.selectLocationButton.button('option', 'label', 'Change');

        that.bar_active = data.active;
    });
};

CreateBarExecution.prototype.saveBar = function () {
    var that = this;

    var __ = this.dialog;

    var data = {
        name: $('input[name=name]', __).val(),
        intro: $('textarea[name=intro]', __).val(),
        description: $('textarea[name=description]', __).val(),
        urlkey: $('input[name=urlkey]', __).val(),
        street1: $('input[name=street1]', __).val(),
        street2: $('input[name=street2]', __).val(),
        city: $('input[name=city]', __).val(),
        county: $('input[name=county]', __).val(),
        postcode: $('input[name=postcode]', __).val(),
        country: $('input[name=country]', __).val(),
        phone: $('input[name=phone]', __).val(),
        fax: $('input[name=fax]', __).val(),
        email: $('input[name=email]', __).val(),
        website: $('input[name=website]', __).val(),
        menuUrl: $('input[name=menu_url]', __).val(),
        openhours: {
            monday: { fromA: $('input[name=mon1]', __).val(), toA: $('input[name=mon2]', __).val(), fromB: $('input[name=mon3]', __).val(), toB: $('input[name=mon4]', __).val() },
            tuesday: { fromA: $('input[name=tue1]', __).val(), toA: $('input[name=tue2]', __).val(), fromB: $('input[name=tue3]', __).val(), toB: $('input[name=tue4]', __).val() },
            wednesday: { fromA: $('input[name=wed1]', __).val(), toA: $('input[name=wed2]', __).val(), fromB: $('input[name=wed3]', __).val(), toB: $('input[name=wed4]', __).val() },
            thursday: { fromA: $('input[name=thurs1]', __).val(), toA: $('input[name=thurs2]', __).val(), fromB: $('input[name=thurs3]', __).val(), toB: $('input[name=thurs4]', __).val() },
            friday: { fromA: $('input[name=fri1]', __).val(), toA: $('input[name=fri2]', __).val(), fromB: $('input[name=fri3]', __).val(), toB: $('input[name=fri4]', __).val() },
            saturday: { fromA: $('input[name=sat1]', __).val(), toA: $('input[name=sat2]', __).val(), fromB: $('input[name=sat3]', __).val(), toB: $('input[name=sat4]', __).val() },
            sunday: { fromA: $('input[name=sun1]', __).val(), toA: $('input[name=sun2]', __).val(), fromB: $('input[name=sun3]', __).val(), toB: $('input[name=sun4]', __).val() }
        }
    };

    if (that.map_latlng) {
        data.geo = {
            lat: that.map_latlng.lat(),
            lng: that.map_latlng.lng()
        };
    }

    if (!that.bar_id) {
        $.ajax({
            url: site_base + 'admin/process/createbar',
            data: data,
            type: 'POST'
        }).success(function (data) {
            that.bar_id = data;
            that.bar_active = false;

            $('.bar-activation button', $(__).parent()).button('option', 'disabled', false);
            $('.bar-activation button', $(__).parent()).button('option', 'label', 'Activate');
            that.selectLocationButton.button('option', 'disabled', false);
            that.selectLocationButton.button('option', 'label', 'Change');

            $(__).dialog('option', 'title', 'Editing bar');
        });
    }
    else {
        $.ajax({
            url: site_base + 'admin/process/editbar/' + that.bar_id,
            data: data,
            type: 'POST'
        }).success(function (data) {
        });
    }
};

CreateBarExecution.prototype.saveFeature = function () {
    var that = this;

    var __ = this.dialog;

    var container = $(that).parents('.feature');
    var id = $(this).attr('data-id');
    var url = site_base + 'admin/process/';
    if (!id) {
        url += '/create-feature/';
    }
    else {
        url += '/edit-feature/' + id;
    }

    alert('save feature');
    console.log(this);
};

CreateBarExecution.prototype.selectLocation = function () {
    var that = this;
    var selected = false;

    var tree = $('<div></div>').jstree({
        'json_data': {
            data: global.locations
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
        title: 'Select a location',
        height: 600,
        modal: true,
        draggable: false,
        resizable: false,
        buttons: {
            'OK': function () {
                var dialog = this;
                if (selected) {
                    Helpers.confirmDialog('Are you sure you want to move this bar to "' + selected.name + '"?', function (response) {
                        if (response == 'yes') {
                            $.ajax({
                                url: site_base + 'admin/process/movebar/' + that.bar_id,
                                data: { parent: selected.id },
                                type: 'POST',
                                success: function (data) {
                                    $(dialog).dialog('close');
                                    $(dialog).remove();

                                    that.loadBar(that.bar_id);
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

CreateBarExecution.prototype.viewBar = function () {
    window.open(location.protocol + '//' + location.host + '/' + this.bar_data.url, '_newtab');
};

CreateBarExecution.prototype.toggleActivation = function () {
    var that = this;

    Helpers.confirmDialog('Are you sure you want to ' + (that.bar_active ? 'de' : '') + 'activate this bar?', function (response) {
        if (response == 'yes') {
            $.ajax({
                url: site_base + 'admin/process/ToggleBarActivation/' + that.bar_id,
                data: { active: (!that.bar_active) ? 'true' : 'false' }
            }).success(function (data) {
                if (data == 'false') {
                    Helpers.alertDialog('Something went wrong.');
                }
                else {
                    that.bar_active = (!!!that.bar_active);
                    $('.bar-activation button.activate', $(that.dialog).parent()).button('option', 'label', (that.bar_active ? 'Deactivate' : 'Activate'));
                    $('.bar-activation button.view').button('option', 'disabled', !that.bar_active);
                }
            });
        }
    });
};

CreateBarExecution.prototype.loadRelated = function (id) {
    var that = this;
    if (!id) { return; }
    $.ajax({
        url: site_base + 'admin/json/getrelated/' + id,
        success: function (data) {
            var container = $('.related .existing .search-results', that.dialog);

            container.empty();

            $(data).each(function (index, element) {
                var result = $('<div class="result" data-id="' + element.id + '"><span class="name">' + element.name + '</span><span class="type">' + element.type + '</span></div>');
                container.append(result);

                $(result).click(function (e) { e.preventDefault(); that.removeRelated($(this).attr('data-id'), this); });
            });
        }
    });
};

CreateBarExecution.prototype.relatedSearch = function (query) {
    var that = this;
    $.ajax({
        url: site_base + 'admin/json/search',
        data: { search: query, limit: 15 },
        success: function (data) {
            var container = $('.related .add .search-results', that.dialog);

            container.empty();

            $(data).each(function (index, element) {
                var result = $('<div class="result" data-id="' + element.id + '"><span class="name">' + element.name + '</span><span class="type">' + element.type + '</span></div>');
                container.append(result);

                $(result).click(function (e) { e.preventDefault(); that.addRelated($(this).attr('data-id'), this); });
            });
        }
    });
};

CreateBarExecution.prototype.addRelated = function (id, elem) {
    var that = this;
    if (that.loading) { return; }
    that.loading = true;
    $.ajax({
        url: site_base + 'admin/process/AddRelated/' + that.bar_id,
        data: { right: id },
        success: function (data) {
            if (data != 'false') {
                $(elem).detach().appendTo($('.related .existing .search-results', that.dialog));
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

CreateBarExecution.prototype.removeRelated = function (id, elem) {
    var that = this;
    if (that.loading) { return; }
    that.loading = true;
    $.ajax({
        url: site_base + 'admin/process/RemoveRelated/' + that.bar_id,
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

CreateBarExecution.prototype.loadReviews = function (id) {
    var that = this;
    $.ajax({
        url: site_base + 'admin/json/GetUserReviewsForBar/' + id,
        success: function (data) {
            var container = $('.reviews', that.dialog);

            container.empty();

            var generateDom = function (review) {
                var dom = $('<div class="review" data-id="' + review.id + '"></div>');

                dom.append('<div class="actions"><button class="approve">Approve</button><button class="reject">Reject</button></div>');
                dom.append('<span class="bar" data-id="' + review.bar.id + '"><a href=' + review.bar.url + ' target="_blank">' + review.bar.name + '</a></span>');

                if (review.user == null) {
                    dom.append('<span class="user">user - Anonymous<span class="user-ip">' + review.ip + '</span></span>');
                }
                else {
                    dom.append('<span class="user" data-id="' + review.user.id + '">user - ' + review.user.name + '<span class="user-ip">' + review.ip + '</span></span>');
                }

                dom.append('<span class="title">' + review.title + '</span><span class="comment">' + review.comment + '</span>');

                dom.append('<div class="rating"><strong>Rating</strong> design: <input class="design" value="' + (review.rating ? (review.rating.design).toFixed(1) : '') + '" /> drinks: <input class="drinks" value="' + (review.rating ? (review.rating.drinks).toFixed(1) : '') + '" /> service: <input class="service" value="' + (review.rating ? (review.rating.service).toFixed(1) : '') + '" /><button class="update">Update</button></div>');

                dom.append('<span class="created">' + new Date(review.created).toString() + '</span>');

                dom.append('<hr />');

                return dom;
            };

            $(data).each(function (index, review) {
                var dom = generateDom(review);

                $('button.approve', dom).button({ disabled: review.active }).click(function (e) { e.preventDefault(); that.approveReview($(this).parent().parent().attr('data-id')); });
                $('button.reject', dom).button({ disabled: !review.active }).click(function (e) { e.preventDefault(); that.rejectReview($(this).parent().parent().attr('data-id')); });
                $('button.update', dom).button().click(function (e) {
                    e.preventDefault();
                    console.log($(this).parent());
                    console.log($('.design', $(this).parent()).val());
                    that.updateReview($(this).parent().parent().attr('data-id'), $('.design', $(this).parent()).val(), $('.drinks', $(this).parent()).val(), $('.service', $(this).parent()).val());
                });

                container.append(dom);
            });
        }
    });
};

CreateBarExecution.prototype.approveReview = function (id) {
    var that = this;

    $.ajax({
        url: site_base + 'admin/process/moderateuserreview/' + id,
        data: { approve: true },
        success: function (data) {
            if (data == 'false') {
                Helpers.alertDialog('Something went wrong.');
            }
            else {
                var dom = $('.review[data-id="' + id + '"]');

                $('button.approve', dom).button('option', 'disabled', true);
                $('button.reject', dom).button('option', 'disabled', false);
            }
        },
        error: function (data) {
            Helpers.alertDialog('Something went wrong.');
        }
    });
};

CreateBarExecution.prototype.rejectReview = function (id) {
    var that = this;

    $.ajax({
        url: site_base + 'admin/process/moderateuserreview/' + id,
        data: { approve: false },
        success: function (data) {
            if (data == 'false') {
                Helpers.alertDialog('Something went wrong.');
            }
            else {
                var dom = $('.review[data-id="' + id + '"]');

                $('button.approve', dom).button('option', 'disabled', false);
                $('button.reject', dom).button('option', 'disabled', true);
            }
        },
        error: function (data) {
            Helpers.alertDialog('Something went wrong.');
        }
    });
};

CreateBarExecution.prototype.updateReview = function (id, design, drinks, service) {
    var that = this;

    $.ajax({
        url: site_base + 'admin/process/updateuserreview/' + id,
        data: { design: design, drinks: drinks, service: service },
        success: function (data) {
            if (data == 'false') {
                Helpers.alertDialog('Something went wrong.');
            }
            else {
            }
        },
        error: function (data) {
            Helpers.alertDialog('Something went wrong.');
        }
    });
};

CreateBarExecution.prototype.loadMedia = function (id) {
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

CreateBarExecution.prototype.deleteMedia = function (source) {
    var that = this;
    Helpers.confirmDialog('Are you sure you want to delete this image?  This removes the file from the filesystem and is irreversible.', function (response) {
        if (response == 'yes') {
            $.ajax({
                url: site_base + 'admin/process/deletemedia/' + that.bar_id,
                data: { reference: source },
                dataType: 'json',
                success: function (data) {
                    that.loadMedia(that.bar_id);
                }
            });
        }
    });
};