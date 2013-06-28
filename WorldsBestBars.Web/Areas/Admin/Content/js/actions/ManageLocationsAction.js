var global = global || {};

global.locations = [];
global.locations_flat = [];

$.ajax({
    url: site_base + 'admin/json/getlocations',
    success: function (data) {
        global.locations = data;
    }
});

$.ajax({
    url: site_base + 'admin/json/getlocationsflat',
    success: function (data) {
        global.locations_flat = data;
    }
});

var ManageLocationsAction = function () {
    this.dialog = false;
    this.tabs = false;
    this.tree = false;
    this.map = false;
};

ManageLocationsAction.prototype.init = function () {
    var that = this;

    var initDialog = function () {
        $.ajax({
            url: site_base + 'admin/dialogs/managelocations'
        }).success(function (data) {
            $(data).dialog({
                autoOpen: false,
                title: 'Manage locations',
                width: 800,
                height: 600,
                resizable: false,
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

                    $('.manage-locations-right-bars', that.dialog).jScrollPane({ autoReinitialise: true });

                    that.tabs.tabs().bind('tabsshow', function (event, ui) {
                        google.maps.event.trigger(that.map, 'resize');
                    });

                    that.tabs.tabs('disable', 1);
                    that.tabs.tabs('disable', 2);
                    that.tabs.tabs('disable', 3);
                    that.tabs.tabs('disable', 4);

                    $('[data-type=location]').live('click', function (e) { e.preventDefault(); that.loadLocation($(this).attr('data-id')); });
                    $('[data-type=orphaned]').live('click', function (e) { e.preventDefault(); that.loadLocation('orphan'); });
                    $('[data-type=no-geo]').live('click', function (e) { e.preventDefault(); that.loadLocation('no-geo'); });
                    $('[data-type=no-images]').live('click', function (e) { e.preventDefault(); that.loadLocation('no-images'); });

                    $('.tree', that.dialog).jstree({
                        selected: 0,
                        plugins: ['themes', 'html_data', 'ui', 'crrm', 'contextmenu', 'dnd'],
                        core: { animation: 0 },
                        ui: { select_limit: 1 },
                        themes: { url: '/areas/admin/content/css/style-jstree.css' }
                    })
                    .bind("create.jstree", that.createLocation)
	                .bind("remove.jstree", that.deleteLocation)
                    .bind("rename.jstree", that.renameLocation)
                    .bind("move_node.jstree", that.moveLocation);

                    $('.scrollable', that.dialog).jScrollPane({ autoReinitialise: true });

                    that.map = new google.maps.Map($('.map', that.dialog)[0], {
                        zoom: 2,
                        center: new google.maps.LatLng(20, 0),
                        mapTypeId: google.maps.MapTypeId.ROADMAP
                    });


                    google.maps.event.addListener(that.map, 'click', function () {
                        $(that.map_infowindows).each(function (index, element) {
                            element.close();
                        });
                    });

                    $('.related .existing', that.dialog).jScrollPane({ autoReinitialise: true });

                    $('.related input[name=search]', that.dialog).keyup(function (e) {
                        var theother = this;
                        clearTimeout($(this).data('search-timer'));
                        $(this).data('search-timer', setTimeout(function () { that.relatedSearch($(theother).val()); }, 500));
                    });
                },
                open: function (event, ui) {
                    google.maps.event.trigger(that.map, 'resize');
                    that.map.setCenter(new google.maps.LatLng(20, 0));
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

ManageLocationsAction.prototype.execute = function () {
    $(this.dialog).dialog('open');
};

ManageLocationsAction.prototype.createLocation = function (e, data) {
    var request_data = {
        name: data.rslt.name,
        parent: $('a:first', data.rslt.parent).attr('data-id')
    };

    $.ajax({
        url: site_base + 'admin/process/createlocation',
        data: request_data,
        type: 'POST',
        success: function (response_data) {
            if (response_data != 'false') {
                $(data.rslt.obj).attr('data-id', response_data);
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

ManageLocationsAction.prototype.deleteLocation = function (e, data) {
    var id = $('a:first', data.rslt.obj).attr('data-id');

    Helpers.alertDialog('This only works for locations that have no bars/child locations attached.', function () {
        $.ajax({
            url: site_base + 'admin/process/deletelocation/' + id,
            type: 'POST',
            success: function (response_data) {
                if (response_data != 'false') {
                    $(data.rslt.obj).attr('data-id', response_data);
                }
                else {
                    $.jstree.rollback(data.rlbk);

                    Helpers.alertDialog('Something went wrong.  You probably tried to delete a location that has child locations/bars.');
                }
            },
            error: function (data) {
                $.jstree.rollback(data.rlbk);
            }
        });
    });
};

ManageLocationsAction.prototype.renameLocation = function (e, data) {
    var id = $('a:first', data.rslt.obj).attr('data-id');
    $.ajax({
        url: site_base + 'admin/process/renamelocation/' + id,
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

ManageLocationsAction.prototype.moveLocation = function (e, data) {
    console.log('move');

    var node = data.rslt.o;
    var new_parent = data.rslt.np;

    var source = { id: $('a:first', node).attr('data-id'), name: $('a:first', node).text().trim() };
    var destination = { id: $('a:first', new_parent).attr('data-id'), name: $('a:first', new_parent).text().trim() };

    Helpers.confirmDialog('Are you sure you want to move "' + source.name + '" to "' + destination.name + '"?', function (response) {
        if (response == 'yes') {
            $.ajax({
                url: site_base + 'admin/process/movelocation/' + source.id,
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

ManageLocationsAction.prototype.loadLocation = function (id) {
    var that = this;
    that.currentLocation = id;

    if (id.length == 36) {
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
    }

    $.ajax({
        url: site_base + 'admin/html/bars',
        data: { location: that.currentLocation || null, alt: id }
    }).success(function (data) {
        var data_container = $('div[data-container-for="' + that.currentLocation + '"]');

        $('textarea[name=intro]', that.dialog).val($('div[data-id=intro]', data_container).text());
        $('textarea[name=synopsis]', that.dialog).val($('div[data-id=synopsis]', data_container).text());
        $('input[name=urlkey]', that.dialog).val($('div[data-id=urlkey]', data_container).text());
        $('a[class=button]', that.dialog).button().click(function (e) { e.preventDefault(); that.saveLocation(); });

        if (id.length != 36) {
            that.tabs.tabs('select', 2);

            //that.tabs.tabs('disable', 0);
            that.tabs.tabs('disable', 1);
            that.tabs.tabs('enable', 2);
            that.tabs.tabs('disable', 3);
            that.tabs.tabs('disable', 4);
        }
        else {
            //that.tabs.tabs('enable', 0);
            that.tabs.tabs('enable', 1);
            that.tabs.tabs('enable', 2);
            that.tabs.tabs('enable', 3);
            that.tabs.tabs('enable', 4);
        }

        var table = $(data).dataTable({
            bPaginate: false,
            bJQueryUI: true,
            bSort: false
        });

        $('.bars-table', that.dialog).empty().append($('<div></div>'));

        $('.bars-table div', that.dialog).append(table);

        $(that.map_markers).each(function (index, element) {
            element.setMap(null);
        });

        that.map_markers = [];
        that.map_infowindows = [];
        var locations = [];
        $('tr[data-lat]', $('.bars-table div', that.dialog)).each(function (index, element) {
            var latlng = new google.maps.LatLng($(element).attr('data-lat'), $(element).attr('data-lng'));
            locations.push(latlng);

            var infowindow = new google.maps.InfoWindow({
                content: '<div class="infowindow"><a href="#" class="action" data-action="edit-bar" data-id="' + $(element).attr('data-id') + '">' + $(element).attr('data-name') + '</a> (click to edit)</div>'
            });

            var marker = new google.maps.Marker({
                position: latlng,
                map: that.map,
                title: $(element).attr('data-name')
            });

            google.maps.event.addListener(marker, 'click', function () {
                $(that.map_infowindows).each(function (index, element) {
                    element.close();
                });
                infowindow.open(that.map, marker);
            });

            that.map_markers.push(marker);
            that.map_infowindows.push(infowindow);
        });

        /** need to find a suitable replacement for this to cluster map markers
        if (instance.map_markerclusterer) {
        instance.map_markerclusterer.setMap(null);
        }
        instance.map_markerclusterer = new MarkerClusterer(instance.map, instance.map_markers);*/

        if (locations.length > 0) {
            var bounds = new google.maps.LatLngBounds();

            $(locations).each(function (index, latlng) {
                bounds.extend(latlng);
            });

            that.map.fitBounds(bounds);
        }
    });
};

ManageLocationsAction.prototype.saveLocation = function () {
    var that = this;

    var data = {
        synopsis: $('textarea[name=synopsis]', that.dialog).val(),
        urlkey: $('input[name=urlkey]', that.dialog).val(),
        intro: $('textarea[name=intro]', that.dialog).val()
    };

    $.ajax({
        url: site_base + 'admin/process/editlocation/' + that.currentLocation,
        data: data,
        type: 'POST'
    }).success(function (data) {
        var data_container = $('div[data-container-for="' + that.currentLocation + '"]');

        $('div[data-id=synopsis]', data_container).text(data.synopsis);
        $('div[data-id=urlkey]', data_container).text(data.urlkey);
        $('div[data-id=intro]', data_container).text(data.intro);
    });
};

ManageLocationsAction.prototype.loadRelated = function (id) {
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

ManageLocationsAction.prototype.relatedSearch = function (query) {
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

ManageLocationsAction.prototype.addRelated = function (id, elem) {
    var that = this;
    if (that.loading) { return; }
    that.loading = true;
    $.ajax({
        url: site_base + 'admin/process/AddRelated/' + that.currentLocation,
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

ManageLocationsAction.prototype.removeRelated = function (id, elem) {
    var that = this;
    if (that.loading) { return; }
    that.loading = true;
    $.ajax({
        url: site_base + 'admin/process/RemoveRelated/' + that.currentLocation,
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

ManageLocationsAction.prototype.loadMedia = function (id) {
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

ManageLocationsAction.prototype.deleteMedia = function (source) {
    var that = this;
    Helpers.confirmDialog('Are you sure you want to delete this image?  This removes the file from the filesystem and is irreversible.', function (response) {
        if (response == 'yes') {
            $.ajax({
                url: site_base + 'admin/process/deletemedia/' + that.currentLocation,
                data: { reference: source },
                dataType: 'json',
                success: function (data) {
                    that.loadMedia(that.currentLocation);
                }
            });
        }
    });
};