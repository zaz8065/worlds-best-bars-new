var ModerateBarResourceSubmissionsAction = function () {
    var that = this;
    this.dialog = false;
    this.data = false;
};

ModerateBarResourceSubmissionsAction.prototype.init = function () {
    var that = this;

    var initDialog = function () {
        $.ajax({
            url: site_base + 'admin/dialogs/ManageBarResourceSubmissions'
        }).success(function (data) {
            that.dialog = $(data);

            that.dialog.dialog({
                title: 'Moderate submissions from Bar Resource',
                autoOpen: false,
                width: 800,
                height: 600,
                resizable: false,
                show: 'fade',
                hide: 'fade',
                create: function (event, ui) {

                    that.map_latlng = new google.maps.LatLng(0, 0);
                    that.map = new google.maps.Map($('.geo', this)[0], {
                        zoom: 2,
                        center: that.map_latlng,
                        mapTypeId: google.maps.MapTypeId.ROADMAP
                    });

                    that.marker = new google.maps.Marker({
                        map: that.map,
                        position: that.map_latlng
                    });

                    that.reloadData();
                }
            });
        });
    };

    initDialog();
};

ModerateBarResourceSubmissionsAction.prototype.execute = function () {
    $(this.dialog).dialog('open');
};

ModerateBarResourceSubmissionsAction.prototype.reloadData = function (selected) {
    var that = this;

    $.ajax({
        url: site_base + 'admin/json/getbarresourcedata'
    }).success(function (data) {
        that.data = data;
        var container = $('.list', that.dialog);
        $(data).each(function () {
            if (!this.error) {
                var html = $('<div></div>');

                html.text((this.is_new ? 'NEW - ' : '') + this.name);
                html.data('id', this.id);
                html.data('raw', this);

                container.append(html);

                html.click(function (e) {
                    e.preventDefault();
                    that.loadEntity($(this).data('id'), $(this).data('raw'));
                });
            }
        });
    });
};

ModerateBarResourceSubmissionsAction.prototype.loadEntity = function (id, data) {
    var that = this;

    if (data.is_new) {
        $('.view_current', this.dialog).attr('data-id', data.bar_id).hide();

        $('.current-name').text('');
        $('.current-website').text('');
        $('.current-email').text('');
        $('.current-address').text('');
        $('.current-phone').text('');
    }
    else {
        $('.view_current', this.dialog).attr('data-id', data.bar_id).show();

        $('.current-name').text(data.current.name);
        $('.current-website').text(data.current.contact.website);
        $('.current-email').text(data.current.contact.email);
        $('.current-address').text(data.current.contact.address);
        $('.current-phone').text(data.current.contact.phone);
    }

    $('.name', this.dialog).val(data.name);
    $('.website', this.dialog).val(data.contact.website);
    $('.phone', this.dialog).val(data.contact.phone);
    $('.email', this.dialog).val(data.contact.email);
    $('.address', this.dialog).val(data.contact.address);
    $('.menu', this.dialog).val(data.menu.url);

    $('.cocktail-name', this.dialog).val(data.cocktail.name);
    $('.cocktail-description', this.dialog).val(data.cocktail.description);
    $('.cocktail-ingredients', this.dialog).val(data.cocktail.ingredients);

    var features = $('.features', this.dialog);
    features.empty();
    $(data.features).each(function () {
        var el = $('<div></div>');

        el.append($('<strong>Title</strong>'));
        el.append($('<input type="text" />').val(this.title));
        el.append($('<strong>Description</strong>'));
        el.append($('<input type="text" />').val(this.description));
        el.append($('<img />').attr('src', this.image).click(function () {
            window.open($(this).attr('src'), '_newtab');
        }));
        el.append('<hr />');

        features.append(el);
    });


    var openHours = $('.open-hours', this.dialog);
    openHours.empty();
    var days = ['Mon', 'Tue', 'Wed', 'Thurs', 'Fri', 'Sat', 'Sun'];
    $(data.open_hours).each(function (index) {
        try
        {
            var el = $('<div></div>');

            el.append($('<strong>' + days[index] + '</strong>'));
            $(this).each(function () {
                el.append('<div>Open: ' + this.open + ' Close: ' + this.close + '</div>');
            });
            el.append('<hr />');

            openHours.append(el);
        }
        catch (ex) {
        }
    });

    $('.pictures', this.dialog).empty();
    $(data.pictures).each(function () {
        var img = document.createElement('img');
        $(img).attr('src', '/content/media/_bar-resource/' + data.id + '/pictures/' + this);
        $(img).click(function () {
            window.open($(img).attr('src'), '_newtab');
        });
        $('.pictures', this.dialog).append(img);
    });

    $('.styles', this.dialog).empty();
    $(data.type.styles).each(function () {
        $('.styles', that.dialog).append($(document.createElement('span')).text(this.toString()));
    });

    $('.specialities', this.dialog).empty();
    $(data.type.specialities).each(function () {
        $('.specialities', that.dialog).append($(document.createElement('span')).text(this.toString()));
    });

    $('.atmosphere', this.dialog).empty();
    $(data.type.atmosphere).each(function () {
        $('.atmosphere', that.dialog).append($(document.createElement('span')).text(this.toString()));
    });

    if (data.contact.geo && data.contact.geo.length) {
        $('.geo', this.dialog).show();
        var pt = new google.maps.LatLng(data.contact.geo[0], data.contact.geo[1]);
        this.marker.setPosition(pt);
        google.maps.event.trigger(this.map, 'resize');
        this.map.panTo(pt);
        this.map.setZoom(16);
    }
    else {
        $('.geo', this.dialog).hide();
    }
}