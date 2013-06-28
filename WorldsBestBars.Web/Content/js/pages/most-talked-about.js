var MostTalkedAboutPage = function () {
    this.pagination = false;

    this.init();
};

MostTalkedAboutPage.prototype.init = function () {
    this.pagination = new Pagination({
        item_selector: '.panel[data-filter-hidden="false"]',
        item_container_id: 'bar_list_container',
        pagination_container_id: 'bar_list_pagination',
        items_per_page: 5
    });

    this.initFilters();
};

MostTalkedAboutPage.prototype.initFilters = function () {
    var that = this;

    var switch_filter = function (filter) {
        if (filter == '-') {
            $('#bar_list_container .panel').show().attr('data-filter-hidden', 'false');
        }
        else {
            $('#bar_list_container .panel[data-city!="' + filter + '"]').hide().attr('data-filter-hidden', 'true');
            $('#bar_list_container .panel[data-city="' + filter + '"]').show().attr('data-filter-hidden', 'false');
        }

        that.pagination.refresh();
    };

    $('#most-talked-about select[name=filter]').change(function (e) {
        switch_filter($(this).val());
    });

    switch_filter('-');
};