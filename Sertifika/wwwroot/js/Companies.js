function CompaniesViewModel() {
    var self = this;

    self.companies = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.editingId = null;
    self.searchQuery = ko.observable('');

    self.formName = ko.observable('');
    self.formContactEmail = ko.observable('');
    self.formContactPhone = ko.observable('');
    self.formAddress = ko.observable('');

    self.filteredCompanies = ko.computed(function() {
        var q = self.searchQuery().toLocaleLowerCase('tr');
        if (!q) return self.companies();
        return self.companies().filter(function(c) {
            return (c.name || '').toLocaleLowerCase('tr').indexOf(q) >= 0 ||
                   (c.contactEmail || '').toLocaleLowerCase('tr').indexOf(q) >= 0 ||
                   (c.contactPhone || '').indexOf(q) >= 0;
        });
    });

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
        self.isEditing(false);
        self.editingId = null;
        self.formName('');
        self.formContactEmail('');
        self.formContactPhone('');
        self.formAddress('');
        formModal.show();
    };

    self.openEditModal = function(company) {
        self.isEditing(true);
        self.editingId = company.id;
        self.formName(company.name || '');
        self.formContactEmail(company.contactEmail || '');
        self.formContactPhone(company.contactPhone || '');
        self.formAddress(company.address || '');
        formModal.show();
    };

    self.saveCompany = function() {
        self.isSaving(true);
        var body = {
            name: self.formName(),
            contactEmail: self.formContactEmail(),
            contactPhone: self.formContactPhone(),
            address: self.formAddress()
        };

        var promise;
        if (self.isEditing()) {
            body.id = self.editingId;
            promise = apiPut('/companies/' + self.editingId, body);
        } else {
            promise = apiPost('/companies', body);
        }

        promise
            .done(function() {
                formModal.hide();
                toastr.success(self.isEditing() ? 'Firma guncellendi' : 'Firma eklendi');
                self.loadData();
            })
            .fail(function() {
                toastr.error('Islem basarisiz');
            })
            .always(function() {
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
