var SearchPage = function () {
    this.result_container = false;
    this.result_count = false;

    this.filter = {
        current_filter: false,
        container: false
    };

    this.init();
};

SearchPage.prototype.init = function () {
    this.pagination = new Pagination({
        item_selector: '.results[data-filter-hidden="false"]',
        item_container_id: 'search_results_list',
        pagination_container_id: 'search_results_pagination'
    });

    this.initFilters();
};

SearchPage.prototype.initFilters = function () {
    var that = this;

    var switch_filter = function (filter) {
        if (filter == 'all') {
            $('div.results', that.result_container).show().attr('data-filter-hidden', 'false');
        }
        else {
            $('div.results:not(.' + filter + ')', that.result_container).hide().attr('data-filter-hidden', 'true');
            $('div.results.' + filter, that.result_container).show().attr('data-filter-hidden', 'false');
        }

        $('[data-filter!="' + filter + '"]', that.filter.container).removeClass('active');
        $('[data-filter="' + filter + '"]', that.filter.container).addClass('active');

        that.pagination.refresh();
    };

    this.filter.container = document.getElementById('results_filter');
    $('[data-filter]', this.filter.container).click(function (e) { e.preventDefault(); switch_filter($(this).attr('data-filter')); });

    switch_filter('all');
};