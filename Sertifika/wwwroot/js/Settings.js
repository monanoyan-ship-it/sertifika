function SettingsViewModel() {
    var self = this;

    self.accounts = ko.observableArray([]);
    self.smtpAccounts = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);

    // OneDrive form
    self.formName = ko.observable('');
    self.formTenantId = ko.observable('');
    self.formClientId = ko.observable('');
    self.formClientSecret = ko.observable('');
    self.formDriveUserId = ko.observable('');
    self.formIsDefault = ko.observable(false);

    // SMTP form
    self.smtpName = ko.observable('');
    self.smtpHost = ko.observable('');
    self.smtpPort = ko.observable(587);
    self.smtpUsername = ko.observable('');
    self.smtpPassword = ko.observable('');
    self.smtpFromEmail = ko.observable('');
    self.smtpFromName = ko.observable('');
    self.smtpUseSsl = ko.observable(true);
    self.smtpIsDefault = ko.observable(false);

    var formModal, smtpModal;

    self.loadData = function() {
        self.isLoading(true);
        $.when(
            apiGet('/onedrive-accounts'),
            apiGet('/smtp-accounts')
        ).done(function(odRes, smtpRes) {
            self.accounts(odRes[0]);
            self.smtpAccounts(smtpRes[0]);
        }).fail(function() {
            toastr.error('Hesaplar yuklenemedi');
        }).always(function() {
            self.isLoading(false);
        });
    };

    self.openCreateModal = function() {
        self.formName('');
        self.formTenantId('');
        self.formClientId('');
        self.formClientSecret('');
        self.formDriveUserId('');
        self.formIsDefault(false);
        formModal.show();
    };

    self.saveAccount = function() {
        self.isSaving(true);
        var body = {
            name: self.formName(),
            tenantId: self.formTenantId(),
            clientId: self.formClientId(),
            clientSecret: self.formClientSecret(),
            driveUserId: self.formDriveUserId(),
            isDefault: self.formIsDefault()
        };
        apiPost('/onedrive-accounts', body)
            .done(function() {
                formModal.hide();
                toastr.success('OneDrive hesabi eklendi');
                self.loadData();
            })
            .fail(function() {
                toastr.error('Hesap eklenemedi');
            })
            .always(function() {
                self.isSaving(false);
            });
    };

    self.setDefault = function(account) {
        apiPost('/onedrive-accounts/' + account.id + '/set-default')
            .done(function() {
                toastr.success('Varsayilan hesap ayarlandi');
                self.loadData();
            })
            .fail(function() { toastr.error('Islem basarisiz'); });
    };

    self.deleteAccount = function(account) {
        showConfirm('OneDrive hesabini silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/onedrive-accounts/' + account.id)
                .done(function() {
                    toastr.success('Hesap silindi');
                    self.loadData();
                })
                .fail(function() { toastr.error('Silinemedi'); });
        });
    };

    // SMTP CRUD
    self.openSmtpCreateModal = function() {
        self.smtpName('');
        self.smtpHost('');
        self.smtpPort(587);
        self.smtpUsername('');
        self.smtpPassword('');
        self.smtpFromEmail('');
        self.smtpFromName('');
        self.smtpUseSsl(true);
        self.smtpIsDefault(false);
        smtpModal.show();
    };

    self.saveSmtpAccount = function() {
        self.isSaving(true);
        var body = {
            name: self.smtpName(),
            host: self.smtpHost(),
            port: self.smtpPort(),
            username: self.smtpUsername(),
            password: self.smtpPassword(),
            fromEmail: self.smtpFromEmail(),
            fromName: self.smtpFromName(),
            useSsl: self.smtpUseSsl(),
            isDefault: self.smtpIsDefault()
        };
        apiPost('/smtp-accounts', body)
            .done(function() {
                smtpModal.hide();
                toastr.success('SMTP hesabi eklendi');
                self.loadData();
            })
            .fail(function() {
                toastr.error('SMTP hesabi eklenemedi');
            })
            .always(function() {
                self.isSaving(false);
            });
    };

    self.setSmtpDefault = function(account) {
        apiPost('/smtp-accounts/' + account.id + '/set-default')
            .done(function() {
                toastr.success('Varsayilan SMTP hesabi ayarlandi');
                self.loadData();
            })
            .fail(function() { toastr.error('Islem basarisiz'); });
    };

    self.deleteSmtpAccount = function(account) {
        showConfirm('SMTP hesabini silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/smtp-accounts/' + account.id)
                .done(function() {
                    toastr.success('SMTP hesabi silindi');
                    self.loadData();
                })
                .fail(function() { toastr.error('Silinemedi'); });
        });
    };

    $(document).ready(function() {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        smtpModal = new bootstrap.Modal(document.getElementById('smtpModal'));
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new SettingsViewModel(), document.getElementById('settingsApp'));
