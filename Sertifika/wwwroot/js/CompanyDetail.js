function CompanyDetailViewModel() {
    var self = this;

    self.detail = ko.observable(null);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.activeTab = ko.observable('info');

    self.isEditingContact = ko.observable(false);
    self.editingContactId = null;
    self.contactFirstName = ko.observable('');
    self.contactLastName = ko.observable('');
    self.contactEmail = ko.observable('');
    self.contactPhone = ko.observable('');

    var contactModal;

    self.totalParticipations = ko.computed(function() {
        var d = self.detail();
        if (!d) return 0;
        return (d.trainings || []).reduce(function(sum, t) { return sum + (t.participantCount || 0); }, 0);
    });

    self.loadDetail = function() {
        self.isLoading(true);
        apiGet('/companies/' + companyId + '/detail')
            .done(function(data) {
                self.detail(data);
                self.isLoading(false);
            })
            .fail(function() { toastr.error('Firma detayi yuklenemedi'); self.isLoading(false); });
    };

    self.openContactModal = function() {
        self.isEditingContact(false);
        self.editingContactId = null;
        self.contactFirstName(''); self.contactLastName('');
        self.contactEmail(''); self.contactPhone('');
        contactModal.show();
    };

    self.editContact = function(c) {
        self.isEditingContact(true);
        self.editingContactId = c.id;
        self.contactFirstName(c.firstName);
        self.contactLastName(c.lastName);
        self.contactEmail(c.email || '');
        self.contactPhone(c.phone || '');
        contactModal.show();
    };

    self.saveContact = function() {
        self.isSaving(true);
        var body = {
            firstName: self.contactFirstName(),
            lastName: self.contactLastName(),
            email: self.contactEmail() || null,
            phone: self.contactPhone() || null
        };
        var promise = self.isEditingContact()
            ? apiPut('/companies/' + companyId + '/contacts/' + self.editingContactId, body)
            : apiPost('/companies/' + companyId + '/contacts', body);
        promise
            .done(function() {
                contactModal.hide();
                toastr.success('Kisi kaydedildi');
                self.loadDetail();
            })
            .fail(function(xhr) { toastr.error('Kayit basarisiz: ' + (xhr.responseText || '')); })
            .always(function() { self.isSaving(false); });
    };

    self.deleteContact = function(c) {
        showConfirm('Kisiyi silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/companies/' + companyId + '/contacts/' + c.id)
                .done(function() { toastr.success('Kisi silindi'); self.loadDetail(); })
                .fail(function() { toastr.error('Silinemedi'); });
        });
    };

    $(document).ready(function() {
        contactModal = new bootstrap.Modal(document.getElementById('contactModal'));
        self.loadDetail();
    });
}

requireAuth() && ko.applyBindings(new CompanyDetailViewModel(), document.getElementById('companyDetailApp'));
