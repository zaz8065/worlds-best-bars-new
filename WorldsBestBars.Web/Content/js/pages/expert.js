var ExpertPage = function () {
    this.pagination = false;

    this.init();
};

ExpertPage.prototype.init = function () {
    this.pagination = new Pagination({
        item_selector: '.panel',
        item_container_id: 'bar_reviews_list',
        pagination_container_id: 'bar_reviews_pagination'
    });
};