function SettingsViewModel() {
    var self = this;

    self.accounts = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.formData = ko.observable({
        name: '', tenantId: '', clientId: '', clientSecret: '', driveUserId: '', isDefault: false
    });

    var formModal;

    self.loadData = function() {
        self.isLoading(true);
        apiGet('/onedrive-accounts')
            .done(function(data) {
                self.accounts(data);
                self.isLoading(false);
            })
            .fail(function() {
                toastr.error('OneDrive hesaplari yuklenemedi');
                self.isLoading(false);
            });
    };

    self.openCreateModal = function() {
        self.formData({ name: '', tenantId: '', clientId: '', clientSecret: '', driveUserId: '', isDefault: false });
        formModal.show();
    };

    self.saveAccount = function() {
        self.isSaving(true);
        apiPost('/onedrive-accounts', self.formData())
            .done(function() {
                formModal.hide();
                toastr.success('OneDrive hesabi eklendi');
                self.loadData();
                self.isSaving(false);
            })
            .fail(function() {
                toastr.error('Hesap eklenemedi');
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

    $(document).ready(function() {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new SettingsViewModel(), document.getElementById('settingsApp'));
