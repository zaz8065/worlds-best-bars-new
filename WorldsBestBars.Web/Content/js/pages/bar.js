var BarPage = function (id, name, url, map_centre, area) {
    this.bar = {
        id: id,
        name: name,
        url: url
    };
    this.pagination = false;
    this.url = '';
    this.map = {
        dom: false,
        api: false,
        marker: false,
        centre: map_centre
    };
	this.area = area;

    this.init();
};

BarPage.prototype.init = function () {
    var that = this;

    this.pagination = new Pagination({
        item_selector: '.user-review',
        item_container_id: 'bar_reviews_list',
        pagination_container_id: 'bar_reviews_pagination'
    });

    this.url = window.location;

    this.initSharing();

    $('a.bar-info, #show_on_map').click(function (e) {
        e.preventDefault();
        fadeallout();
        $('#bar-info_overlay').fadeIn();
        that.initMap();
    });

    $('a.signature-drink').click(function (e) {
        e.preventDefault();
        fadeallout();
        $('.popup-cocktail').fadeIn();
    });

    $('a.features').click(function (e) {
        e.preventDefault();
        fadeallout();
        $('.popup-features').fadeIn();
    });

    $('#bar_info_close').click(function (e) {
        e.preventDefault();
        $('#bar-info_overlay').fadeOut('fast');
        fadeallin();
    });

    function limitText(limitField, limitCount, limitNum) {
        if (limitField.value.length > limitNum) {
            limitField.value = limitField.value.substring(0, limitNum);
        } else {
            limitCount.value = limitNum - limitField.value.length;
        }
    }

    function updateCountdown() {

        // 140 is the max message length

        var maxlen = parseInt($(this).attr('maxlength'));

        var remaining = maxlen - $(this).val().length;

        $(this).prev('span').text('(' + remaining + ' characters ).');

        limitText(this, remaining, maxlen - 1);


    }

    $('#review_title').change(updateCountdown).keyup(updateCountdown);
    $('#review_comments').change(updateCountdown).keyup(updateCountdown);
    $('.reply_text').change(updateCountdown).keyup(updateCountdown);

    $('#bar-info_overlay ul.bar-sub-nav a.read-review').click(function (e) {
        e.preventDefault();
        $('#bar-info_overlay').hide();
        fadeallin();
        hideMap();
    });

    $('#bar-info_overlay ul.bar-sub-nav .add-review').click(function (e) {
        e.preventDefault();
        $('#bar-info_overlay').hide();
        fadeallin();
        hideMap();
    });


    $(function () {
        if (location.hash) {
            $('a[href="' + location.hash + '"]').click();
        }
    });

};

BarPage.prototype.initSharing = function () {
    var that = this;

    var close_dialog = function () {
        $('.bar-sharing .share_hover').fadeOut('fast');
    };

    var open_dialog = function () {
        $('.bar-sharing .share_hover').fadeIn('fast');
    };

    var record_share = function (medium) {
        if (_gaq) {
            _gaq.push(['_trackEvent', 'share', medium, that.bar.url]);
        }
    };

    var share_facebook = function (reviewed) {
        var share_url = 'http://www.facebook.com/sharer.php?u=' + encodeURIComponent(that.url);
        window.open(share_url, 'sharer', 'toolbar=0,status=0,width=649,height=436,left=50,top=50');

        record_share('facebook');
    };

    var share_twitter = function (reviewed) {
        var text = '';
		if (that.area != '') {
			var _area = that.area.replace(' ', '').toLowerCase();
			_area = '#' + _area;
		}
        if (reviewed) {
            text = 'I just reviewed ' + that.bar.name + ' on @WorldsBestBars #discoverbars ' + (_area ? _area + ' ' : '') + '- Check it out here ';
        }
        else {
            text = 'One of the @WorldsBestBars #discoverbars ' + (_area ? _area + ' ' : '') + '';
        }
        var share_url = 'https://twitter.com/share?text=' + encodeURIComponent(text);

        window.open(share_url, 'sharer', 'toolbar=0,status=0,width=626,height=251,left=50,top=50');

        record_share('twitter');
    };

    var share_digg = function () {
        var share_url = 'http://digg.com/submit?url=' + encodeURIComponent(that.url);
        window.open(share_url, '_newtab');
        record_share('digg');
    };

    var share_delicious = function () {
        var share_url = 'http://delicious.com/save?url=' + encodeURIComponent(that.url);
        window.open(share_url, '_newtab');
        record_share('delicious');
    };

    var share_reddit = function () {
        var share_url = 'http://www.reddit.com/submit?url=' + encodeURIComponent(that.url);
        window.open(share_url, '_newtab');
        record_share('reddit');
    };

    var share_stumble = function () {
        var share_url = 'http://www.stumbleupon.com/share?url=' + encodeURIComponent(that.url);
        window.open(share_url, '_newtab');
        record_share('stumbleupon');
    };

    var share_print = function () {
        window.print();
        record_share('print');
    };

    var share_email = function () {
        $('.email_popup').fadeIn('fast', function () {
            $('input[name="recipient"]').focus();
        });

        record_share('email');
    };

    $('.bar-sharing .fb').click(function (e) {
        e.preventDefault();
        share_facebook($(this).parents('.review_popup').length > 0);
    });

    $('.bar-sharing .twit').click(function (e) {
        e.preventDefault();
        share_twitter($(this).parents('.review_popup').length > 0);
    });

    $('.bar-sharing .print').click(function (e) {
        e.preventDefault();
        share_print();
        close_dialog();
    });

    $('.bar-sharing .digg').click(function (e) {
        e.preventDefault();
        share_digg();
        close_dialog();
    });

    $('.bar-sharing .del').click(function (e) {
        e.preventDefault();
        share_delicious();
        close_dialog();
    });

    $('.bar-sharing .reddit').click(function (e) {
        e.preventDefault();
        share_reddit();
        close_dialog();
    });

    $('.bar-sharing .stumble').click(function (e) {
        e.preventDefault();
        share_stumble();
        close_dialog();
    });

    $('.bar-sharing .email').click(function (e) {
        e.preventDefault();
        share_email();
        close_dialog();
    });

    $('.bar-sharing .share').click(function (e) {
        e.preventDefault();
        open_dialog();
    });

    $('.bar-sharing .share_hover .close').click(function (e) {
        e.preventDefault();
        close_dialog();
    });

    $(window).keydown(function (e) {
        if (e.keyCode == 27 && $('.bar-sharing .share_hover').is(':visible')) {
            e.preventDefault();
            close_dialog();
        }
    });
};

BarPage.prototype.initMap = function () {
    if (this.map.centre && !this.map.dom) {
        this.map.dom = document.getElementById("map_canvas");
        this.map.api = new google.maps.Map(this.map.dom, {
            zoom: 14,
            center: this.map.centre,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        });

        this.map.marker = new google.maps.Marker({ position: this.map.centre, map: this.map.api });
    }
};