var LocationAreaPage = function (id) {
    this.id = id;
    this.pagination = false;

    this.init();
};

LocationAreaPage.prototype.init = function () {
    this.pagination = new Pagination({
        item_selector: '.panel',
        item_container_id: 'bar_list_container',
        pagination_container_id: 'bar_list_pagination'
    });
};