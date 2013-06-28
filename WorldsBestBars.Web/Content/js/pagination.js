var Pagination = function (opts) {
    this.item_container = false;
    this.item_count = false;
    this.item_selector = opts.item_selector;
    this.item_container_id = opts.item_container_id;

    this.pagination = {
        items_per_page: opts.items_per_page || 4,
        pages: false,
        current_page: 1,
        container: false,
        container_id: opts.pagination_container_id
    };

    this.init();
};

Pagination.prototype.init = function () {
    this.item_container = document.getElementById(this.item_container_id);
    this.pagination.container = document.getElementById(this.pagination.container_id);

    this.refresh();
};

Pagination.prototype.refresh = function () {
    var that = this;

    that.item_count = $(that.item_selector, that.item_container).length;
    that.pagination.pages = Math.ceil(that.item_count / that.pagination.items_per_page);
    if (that.item_count == 0 || that.pagination.pages == 1) {
        $(that.pagination.container).hide();
    }
    else {
        if (!that.pagination.current_page) {
            that.pagination.current_page = 1;
        }
        else {
            that.pagination.current_page = that.pagination.current_page < that.pagination.pages ? that.pagination.current_page : that.pagination.pages - 1;
        }


        $(that.item_selector, that.item_container).each(function (index, element) {
            $(element).attr('data-page', Math.floor(index / that.pagination.items_per_page) + 1);
        });

        var switch_page = function (index) {
            that.pagination.current_page = index;
            $(that.item_selector + '[data-page]:not([data-page=' + that.pagination.current_page + '])', that.item_container).hide();
            $(that.item_selector + '[data-page=' + that.pagination.current_page + ']', that.item_container).show();

            $('a[data-page]', that.pagination.container).removeClass('active');
            $('a[data-page=' + that.pagination.current_page + ']', that.pagination.container).addClass('active');
        };

        $('.pages', that.pagination.container).empty();
        for (var i = 0; i < that.pagination.pages; i++) {
            var dom = $('<span><a href="#" data-page="' + (i + 1) + '">' + (i + 1) + '</a>&nbsp;|&nbsp;</span>');
            $('a', dom).click(function (e) { e.preventDefault(); switch_page(parseInt($(this).attr('data-page'))); });
            $('.pages', that.pagination.container).append(dom);
        };

        $('#prev', that.pagination.container).unbind('click').click(function (e) { e.preventDefault(); if (that.pagination.current_page > 1) { switch_page(that.pagination.current_page - 1); } });
        $('#next', that.pagination.container).unbind('click').click(function (e) { e.preventDefault(); if (that.pagination.current_page < that.pagination.pages) { switch_page(that.pagination.current_page + 1); } });

        $(that.pagination.container).show();

        switch_page(that.pagination.current_page);
    }
};