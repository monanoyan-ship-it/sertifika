function TemplatesViewModel() {
    var self = this;

    self.templates = ko.observableArray([]);
    self.isLoading = ko.observable(true);

    self.loadData = function() {
        self.isLoading(true);
        apiGet('/templates')
            .done(function(data) {
                self.templates(data);
                self.isLoading(false);
            })
            .fail(function() {
                toastr.error('Sablonlar yuklenemedi');
                self.isLoading(false);
            });
    };

    self.deleteTemplate = function(tpl) {
        showConfirm('Sablonu silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/templates/' + tpl.id)
                .done(function() {
                    toastr.success('Sablon silindi');
                    self.loadData();
                })
                .fail(function() { toastr.error('Silinemedi'); });
        });
    };

    $(document).ready(function() {
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new TemplatesViewModel(), document.getElementById('templatesApp'));
