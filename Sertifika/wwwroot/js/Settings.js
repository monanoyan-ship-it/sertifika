function SettingsViewModel() {
    var self = this;

    self.accounts = ko.observableArray([]);
    self.smtpAccounts = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isTesting = ko.observable(false);

    // OneDrive form
    self.formName = ko.observable('');
    self.formTenantId = ko.observable('');
    self.formClientId = ko.observable('');
    self.formClientSecret = ko.observable('');
    self.formDriveUserId = ko.observable('');
    self.formIsDefault = ko.observable(false);

    // OAuth state
    self.odTab = ko.observable('easy');
    self.odConnected = ko.observable(false);
    self.odAuthLoading = ko.observable(false);
    self.odDrives = ko.observableArray([]);
    self.odSelectedDrive = ko.observable('');
    self.odRefreshToken = ko.observable('');

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

    // ─── OneDrive OAuth Flow ───

    self.openCreateModal = function() {
        self.formName('');
        self.formTenantId('');
        self.formClientId('');
        self.formClientSecret('');
        self.formDriveUserId('');
        self.formIsDefault(false);
        self.odTab('easy');
        self.odConnected(false);
        self.odDrives([]);
        self.odSelectedDrive('');
        self.odRefreshToken('');
        formModal.show();
    };

    self.startOAuth = function() {
        self.odAuthLoading(true);
        var tenantParam = self.formTenantId() ? '?tenantId=' + encodeURIComponent(self.formTenantId()) : '';
        apiGet('/onedrive-accounts/oauth/auth-url' + tenantParam)
            .done(function(res) {
                if (res.authUrl) {
                    openOAuthPopup(res.authUrl, function(code) {
                        if (code) {
                            self.exchangeCode(code);
                        } else {
                            toastr.error('Microsoft yetkilendirme iptal edildi');
                            self.odAuthLoading(false);
                        }
                    });
                } else {
                    toastr.error('OAuth URL alinamadi');
                    self.odAuthLoading(false);
                }
            })
            .fail(function(xhr) {
                var err = xhr.responseJSON;
                toastr.error(err && err.error ? err.error : 'OAuth baslatma hatasi');
                self.odAuthLoading(false);
            });
    };

    self.exchangeCode = function(code) {
        apiPost('/onedrive-accounts/oauth/exchange-code', {
            code: code,
            tenantId: self.formTenantId() || null
        })
        .done(function(res) {
            if (res.success) {
                self.odRefreshToken(res.refreshToken);
                self.odDrives(res.drives || []);
                if (res.drives && res.drives.length > 0) {
                    self.odSelectedDrive(res.drives[0].driveId);
                }
                self.odConnected(true);
                toastr.success('Microsoft hesabi baglandi! Drive secin ve kaydedin.');
            } else {
                toastr.error(res.error || 'Token alinamadi');
            }
        })
        .fail(function() {
            toastr.error('Token exchange hatasi');
        })
        .always(function() {
            self.odAuthLoading(false);
        });
    };

    self.reconnect = function() {
        self.odConnected(false);
        self.odDrives([]);
        self.odSelectedDrive('');
        self.odRefreshToken('');
    };

    self.saveOAuthAccount = function() {
        if (!self.formName()) {
            toastr.warning('Hesap adi girin');
            return;
        }
        if (!self.odSelectedDrive()) {
            toastr.warning('Drive secin');
            return;
        }
        self.isSaving(true);
        apiPost('/onedrive-accounts/oauth/save', {
            name: self.formName(),
            tenantId: self.formTenantId() || null,
            refreshToken: self.odRefreshToken(),
            driveId: self.odSelectedDrive(),
            isDefault: self.formIsDefault()
        })
        .done(function(res) {
            if (res.success) {
                formModal.hide();
                toastr.success('OneDrive hesabi baglandi!');
                self.loadData();
            } else {
                toastr.error('Hesap kaydedilemedi');
            }
        })
        .fail(function() {
            toastr.error('Hesap kaydedilemedi');
        })
        .always(function() {
            self.isSaving(false);
        });
    };

    // ─── OneDrive Manual (Advanced) ───

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

    self.testConnection = function() {
        self.isTesting(true);
        apiPost('/onedrive-accounts/test-connection')
            .done(function(res) {
                if (res.success) {
                    toastr.success('OneDrive baglantisi basarili!');
                } else {
                    toastr.error('Baglanti hatasi: ' + (res.error || 'Bilinmeyen hata'));
                }
            })
            .fail(function() {
                toastr.error('Baglanti testi basarisiz');
            })
            .always(function() {
                self.isTesting(false);
            });
    };

    // ─── SMTP CRUD ───

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

// ─── OAuth Popup Helper ───

function openOAuthPopup(authUrl, callback) {
    var w = 600, h = 700;
    var left = (screen.width - w) / 2;
    var top = (screen.height - h) / 2;
    var popup = window.open(authUrl, 'OAuthPopup', 'width=' + w + ',height=' + h + ',left=' + left + ',top=' + top);

    var handler = function(event) {
        if (!event.data || typeof event.data !== 'object') return;
        if (event.data.type === 'onedrive-auth-success') {
            window.removeEventListener('message', handler);
            callback(event.data.code);
        } else if (event.data.type === 'onedrive-auth-error') {
            window.removeEventListener('message', handler);
            callback(null);
        }
    };
    window.addEventListener('message', handler);

    var check = setInterval(function() {
        if (!popup || popup.closed) {
            clearInterval(check);
            window.removeEventListener('message', handler);
        }
    }, 1000);
}

requireAuth() && ko.applyBindings(new SettingsViewModel(), document.getElementById('settingsApp'));
