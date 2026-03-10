function EmailTemplatesViewModel() {
    var self = this;

    self.templates = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.editingId = null;

    self.formName = ko.observable('');
    self.formSubject = ko.observable('');
    self.formBody = ko.observable('');
    self.formIsDefault = ko.observable(false);

    self.placeholders = ko.observableArray([
        { code: '{RecipientName}', label: 'Alici Ad Soyad' },
        { code: '{TrainingName}', label: 'Egitim Adi' },
        { code: '{TrainingDate}', label: 'Egitim Tarihi' },
        { code: '{CompanyName}', label: 'Firma Adi' },
        { code: '{CertificateNo}', label: 'Sertifika No' }
    ]);

    self.previewHtml = ko.computed(function() {
        var html = self.formBody();
        if (!html) return '<p class="text-muted">Govde yazildikca onizleme burada gorunur</p>';
        return html
            .replace(/\{RecipientName\}/g, 'Ahmet Yilmaz')
            .replace(/\{TrainingName\}/g, 'Ornek Egitim')
            .replace(/\{TrainingDate\}/g, '15.03.2026')
            .replace(/\{CompanyName\}/g, 'ABC Sirket')
            .replace(/\{CertificateNo\}/g, 'CERT-2026-001');
    });

    var formModal;

    self.loadData = function() {
        self.isLoading(true);
        apiGet('/email-templates')
            .done(function(data) {
                self.templates(data);
                self.isLoading(false);
            })
            .fail(function() {
                toastr.error('Sablonlar yuklenemedi');
                self.isLoading(false);
            });
    };

    self.openCreateModal = function() {
        self.isEditing(false);
        self.editingId = null;
        self.formName('');
        self.formSubject('');
        self.formBody('');
        self.formIsDefault(false);
        formModal.show();
    };

    self.openEditModal = function(tpl) {
        self.isEditing(true);
        self.editingId = tpl.id;
        self.formName(tpl.name);
        self.formSubject(tpl.subject);
        self.formBody(tpl.body);
        self.formIsDefault(tpl.isDefault);
        formModal.show();
    };

    self.saveTemplate = function() {
        self.isSaving(true);
        var body = {
            name: self.formName(),
            subject: self.formSubject(),
            body: self.formBody(),
            isDefault: self.formIsDefault()
        };

        var promise;
        if (self.isEditing()) {
            body.id = self.editingId;
            promise = apiPut('/email-templates/' + self.editingId, body);
        } else {
            promise = apiPost('/email-templates', body);
        }

        promise
            .done(function() {
                formModal.hide();
                toastr.success(self.isEditing() ? 'Sablon guncellendi' : 'Sablon eklendi');
                self.loadData();
            })
            .fail(function() {
                toastr.error('Islem basarisiz');
            })
            .always(function() {
                self.isSaving(false);
            });
    };

    self.setDefault = function(tpl) {
        apiPost('/email-templates/' + tpl.id + '/set-default')
            .done(function() {
                toastr.success('Varsayilan sablon ayarlandi');
                self.loadData();
            })
            .fail(function() { toastr.error('Islem basarisiz'); });
    };

    self.deleteTemplate = function(tpl) {
        showConfirm('Sablonu silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/email-templates/' + tpl.id)
                .done(function() {
                    toastr.success('Sablon silindi');
                    self.loadData();
                })
                .fail(function() { toastr.error('Silinemedi'); });
        });
    };

    self.insertPlaceholder = function(ph) {
        var textarea = document.querySelector('#formModal textarea');
        if (textarea) {
            var start = textarea.selectionStart;
            var end = textarea.selectionEnd;
            var val = self.formBody();
            self.formBody(val.substring(0, start) + ph.code + val.substring(end));
            textarea.focus();
            var pos = start + ph.code.length;
            setTimeout(function() { textarea.setSelectionRange(pos, pos); }, 0);
        }
    };

    $(document).ready(function() {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new EmailTemplatesViewModel(), document.getElementById('emailTemplatesApp'));
