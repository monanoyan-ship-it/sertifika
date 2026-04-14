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

    var formModal, importModal;

    // CSV Import state
    self.availableTemplates = ko.observableArray([]);
    self.importTemplateId = ko.observable(null);
    self.previewResult = ko.observable(null);
    self.confirmResult = ko.observable(null);
    self.isImporting = ko.observable(false);

    self.formatDateRange = function(s, e) { return formatDateRange(s, e); };

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

    // ─── CSV Import ───

    self.openImportModal = function() {
        self.previewResult(null);
        self.confirmResult(null);
        self.importTemplateId(null);
        document.getElementById('import-csv-file').value = '';

        apiGet('/templates')
            .done(function(data) {
                self.availableTemplates(data || []);
                importModal.show();
            })
            .fail(function() { toastr.error('Sablonlar yuklenemedi'); });
    };

    self.downloadImportTemplate = function() {
        downloadAuthedFile(API_BASE + '/companies/import/template', 'firma_import_sablonu.csv');
    };

    self.runPreview = function() {
        var file = document.getElementById('import-csv-file').files[0];
        if (!file) { toastr.warning('CSV dosyasi secin'); return; }

        self.isImporting(true);
        self.previewResult(null);
        self.confirmResult(null);

        var formData = new FormData();
        formData.append('file', file);

        apiPost('/companies/import/preview', formData, true)
            .done(function(res) { self.previewResult(res); })
            .fail(function(xhr) {
                toastr.error('Onizleme basarisiz: ' + (xhr.responseText || ''));
            })
            .always(function() { self.isImporting(false); });
    };

    self.runConfirm = function() {
        var file = document.getElementById('import-csv-file').files[0];
        if (!file) { toastr.warning('CSV dosyasi secin'); return; }
        if (!self.importTemplateId()) { toastr.warning('Varsayilan sablon secin'); return; }

        self.isImporting(true);
        var formData = new FormData();
        formData.append('file', file);
        formData.append('templateId', self.importTemplateId());

        apiPost('/companies/import/confirm', formData, true)
            .done(function(res) {
                self.confirmResult(res);
                toastr.success('Import tamamlandi');
                self.loadData();
            })
            .fail(function(xhr) {
                toastr.error('Import basarisiz: ' + (xhr.responseText || ''));
            })
            .always(function() { self.isImporting(false); });
    };

    $(document).ready(function() {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        importModal = new bootstrap.Modal(document.getElementById('importModal'));
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new CompaniesViewModel(), document.getElementById('companiesApp'));
