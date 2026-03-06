function CompaniesViewModel() {
    var self = this;

    self.companies = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.formData = ko.observable({ name: '', contactEmail: '', contactPhone: '', address: '' });

    var formModal;

    self.loadData = function() {
        self.isLoading(true);
        apiGet('/companies')
            .done(function(data) {
                self.companies(data);
                self.isLoading(false);
            })
            .fail(function() {
                toastr.error('Firmalar yuklenemedi');
                self.isLoading(false);
            });
    };

    self.openCreateModal = function() {
        self.formData({ name: '', contactEmail: '', contactPhone: '', address: '' });
        formModal.show();
    };

    self.saveCompany = function() {
        self.isSaving(true);
        apiPost('/companies', self.formData())
            .done(function() {
                formModal.hide();
                toastr.success('Firma eklendi');
                self.loadData();
                self.isSaving(false);
            })
            .fail(function() {
                toastr.error('Firma eklenemedi');
                self.isSaving(false);
            });
    };

    self.deleteCompany = function(company) {
        showConfirm('Firmayi silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/companies/' + company.id)
                .done(function() {
                    toastr.success('Firma silindi');
                    self.loadData();
                })
                .fail(function() { toastr.error('Silinemedi'); });
        });
    };

    $(document).ready(function() {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new CompaniesViewModel(), document.getElementById('companiesApp'));
