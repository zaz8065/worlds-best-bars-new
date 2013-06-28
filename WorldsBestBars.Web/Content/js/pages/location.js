var LocationPage = function (id) {
    this.id = id;
    this.data = false;
    this.map = false;
    this.map_markers = [];
    this.pagination = false;

    this.init();
};

LocationPage.prototype.init = function () {
    this.initMap();
    this.pagination = new Pagination({
        item_selector: '.panel[data-filter-hidden="false"]',
        item_container_id: 'bar_list_container',
        pagination_container_id: 'bar_list_pagination'
    });
};

LocationPage.prototype.initMap = function () {
    var that = this;

    $.ajax({
        url: '/json/mappoints/' + that.id,
        success: function (data) {
            that.data = data;
            $('#map img').fadeOut('slow', function () {
                $('#map img').remove();
                that.map = new google.maps.Map(document.getElementById('map'), {
                    center: new google.maps.LatLng(20, 0),
                    mapTypeId: google.maps.MapTypeId.ROADMAP
                });

                google.maps.event.addListener(that.map, 'click', function () {
                    $('#bar_list_container .panel').show().attr('data-filter-hidden', 'false');

                    that.pagination.refresh();
                });

                var bounds = new google.maps.LatLngBounds();
                var icon = 'http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=|aa8e3c';
                var shadow = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_shadow", new google.maps.Size(40, 37), new google.maps.Point(0, 0), new google.maps.Point(12, 36));
                $(that.data).each(function (index, bar) {
                    var latlng = new google.maps.LatLng(bar.geo.lat, bar.geo.long);
                    bounds.extend(latlng);

                    var marker = new google.maps.Marker({
                        position: latlng,
                        map: that.map,
                        icon: new google.maps.MarkerImage(icon),
                        shadow: shadow,
                        _bar: bar
                    });

                    google.maps.event.addListener(marker, 'click', function () {
                        $('#bar_list_container .panel[data-id!="' + marker._bar.id + '"]').hide().attr('data-filter-hidden', 'true');
                        $('#bar_list_container .panel[data-id="' + marker._bar.id + '"]').show().attr('data-filter-hidden', 'false');

                        that.pagination.refresh();

                        $(document).stop();
                        $(document).scrollTo('#bar_list_container', 1000);
                    });

                    that.map_markers.push(marker);
                });

                that.map.fitBounds(bounds);
            });
        }
    });
};