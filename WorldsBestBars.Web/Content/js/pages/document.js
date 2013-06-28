var DocumentPage = function (total_items) {
	if (total_items) {
		this.total_items = total_items;
		this.page_start = 0;
		this.items_per_page = 6;
		this.pagination = false;
		this.loading = false;
		
		this.init();
	}

    this.initSharing();
};

DocumentPage.prototype.init = function () {
    var that = this;

    $('.pagination #next').click(function (e) { e.preventDefault(); that.go('next'); });
    $('.pagination #prev').click(function (e) { e.preventDefault(); that.go('prev'); });
};

DocumentPage.prototype.go = function (where) {
    var that = this;
    this.loading = true;

    var reload = function () {
        $.ajax({
            url: '/json/articles',
            data: {
                start: that.page_start,
                count: that.items_per_page
            },
            success: function (data) {
                var template = document.getElementById('document_template').innerHTML.trim();
                var dom = $('<div class="main_box"></div>');

                for (var i = 0; i < data.length; i++) {
                    var html = template;
                    html = html.replace(/{title}/g, data[i].name);
                    html = html.replace(/{url}/g, data[i].url);
                    html = html.replace(/{image}/g, data[i].image);
                    html = html.replace(/{synopsis}/g, data[i].synopsis);

                    dom.append(html);
                }

                $('.main_box').replaceWith(dom);

                that.loading = false;
            },
            error: function (data) {
                that.loading = false;
            }
        });
    };

    switch (where) {
        case 'prev':
            this.page_start -= this.items_per_page;
            if (this.page_start < 0) { this.page_start = 0; }

            break;
        case 'next':
            this.page_start += this.items_per_page;
            if (this.page_start > this.total_items) { this.page_start = total_items - this.items_per_page; }
            break;
    }

    reload();
};

DocumentPage.prototype.initSharing = function () {
    var that = this;

    var close_dialog = function () {
        $('.bar-sharing .share_hover').fadeOut('fast');
    };

    var open_dialog = function () {
        $('.bar-sharing .share_hover').fadeIn('fast');
    };

    var record_share = function (medium) {
        if (_gaq) {
            _gaq.push(['_trackEvent', 'share', medium, window.location.href]);
        }
    };

    var share_facebook = function () {
        var share_url = 'http://www.facebook.com/sharer.php?u=' + encodeURIComponent(window.location.href);
        window.open(share_url, 'sharer', 'toolbar=0,status=0,width=649,height=436,left=50,top=50');

        record_share('facebook');
    };

    var share_twitter = function () {
        var text = 'One of the @WorldsBestBars ';
        var share_url = 'https://twitter.com/share?text=' + encodeURIComponent(text);

        window.open(share_url, 'sharer', 'toolbar=0,status=0,width=626,height=251,left=50,top=50');

        record_share('twitter');
    };

    var share_digg = function () {
        var share_url = 'http://digg.com/submit?url=' + encodeURIComponent(window.location.href);
        window.open(share_url, '_newtab');
        record_share('digg');
    };

    var share_delicious = function () {
        var share_url = 'http://delicious.com/save?url=' + encodeURIComponent(window.location.href);
        window.open(share_url, '_newtab');
        record_share('delicious');
    };

    var share_reddit = function () {
        var share_url = 'http://www.reddit.com/submit?url=' + encodeURIComponent(window.location.href);
        window.open(share_url, '_newtab');
        record_share('reddit');
    };

    var share_stumble = function () {
        var share_url = 'http://www.stumbleupon.com/share?url=' + encodeURIComponent(window.location.href);
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
        share_facebook();
    });

    $('.bar-sharing .twit').click(function (e) {
        e.preventDefault();
        share_twitter();
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