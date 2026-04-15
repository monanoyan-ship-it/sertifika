function SettingsViewModel() {
    var self = this;

    self.accounts = ko.observableArray([]);
    self.smtpAccounts = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.testingAccountId = ko.observable(0);

    // ─── OneDrive form ───
    self.isEditingOd = ko.observable(false);
    self.editingOdId = null;
    self.formName = ko.observable('');
    self.formTenantId = ko.observable('');
    self.formClientId = ko.observable('');
    self.formClientSecret = ko.observable('');
    self.formDriveUserId = ko.observable('');
    self.formDriveId = ko.observable('');
    self.formBasePath = ko.observable('Sertifikalar');
    self.formIsDefault = ko.observable(false);
    self.formIsEnabled = ko.observable(true);
    // Setup mod: "easy" (OAuth), "medium" (Tenant + OAuth), "advanced" (tam manuel)
    self.odSetupMode = ko.observable('easy');

    // OAuth state
    self.odTab = ko.observable('easy');
    self.odConnected = ko.observable(false);
    self.odAuthLoading = ko.observable(false);
    self.odDrives = ko.observableArray([]);
    self.odSelectedDrive = ko.observable('');
    self.odRefreshToken = ko.observable('');
    self.odReconnectId = ko.observable(0); // 0 = create, >0 = reconnect

    // ─── SMTP form ───
    self.isEditingSmtp = ko.observable(false);
    self.editingSmtpId = null;
    self.smtpName = ko.observable('');
    self.smtpHost = ko.observable('');
    self.smtpPort = ko.observable(587);
    self.smtpUsername = ko.observable('');
    self.smtpPassword = ko.observable('');
    self.smtpPasswordHelp = ko.observable('');
    self.smtpFromEmail = ko.observable('');
    self.smtpFromName = ko.observable('');
    self.smtpUseSsl = ko.observable(true);
    self.smtpIsDefault = ko.observable(false);
    self.smtpIsEnabled = ko.observable(true);
    // Auth mode: "basic" | "oauth" | "graph"
    self.smtpAuthMode = ko.observable('basic');
    self.smtpTenantId = ko.observable('');
    self.smtpClientId = ko.observable('');
    self.smtpClientSecret = ko.observable('');
    self.smtpClientSecretHelp = ko.observable('');
    self.smtpIsOAuthMode = ko.computed(function() { return self.smtpAuthMode() === 'oauth' || self.smtpAuthMode() === 'graph'; });
    self.smtpIsGraphMode = ko.computed(function() { return self.smtpAuthMode() === 'graph'; });
    self.smtpAuthModeLabel = function(mode) {
        return { basic: 'Basic Auth', oauth: 'SMTP OAuth 2.0', graph: 'Microsoft Graph API' }[mode] || mode;
    };
    self.smtpAuthModeBadge = function(mode) {
        return { basic: 'bg-secondary', oauth: 'bg-primary', graph: 'bg-info' }[mode] || 'bg-secondary';
    };

    // Send test
    self.sendTestAccountId = ko.observable(0);
    self.sendTestEmail = ko.observable('');
    self.sendTestAccountName = ko.observable('');

    var formModal, smtpModal, userModal, passwordModal, sendTestModal;

    // ─── Tabs & user management state ───
    self.activeTab = ko.observable('users');
    self.isAdmin = ko.observable(false);

    self.users = ko.observableArray([]);
    self.isEditingUser = ko.observable(false);
    self.isSavingUser = ko.observable(false);
    self.editingUserId = null;

    self.userFirstName = ko.observable('');
    self.userLastName = ko.observable('');
    self.userEmail = ko.observable('');
    self.userPassword = ko.observable('');
    self.userRole = ko.observable('Viewer');

    self.newPassword = ko.observable('');
    self.passwordUserLabel = ko.observable('');
    self.passwordUserId = null;

    self.roleLabel = function(role) {
        return { Admin: 'Admin', CertificateCreator: 'Operator', Viewer: 'Goruntuleyici' }[role] || role;
    };

    // ─── Formatters ───

    self.formatLastTest = function(account) {
        if (!account.lastTestedAt) return 'Hic test edilmedi';
        var d = new Date(account.lastTestedAt);
        var now = new Date();
        var diffMs = now - d;
        var diffMin = Math.floor(diffMs / 60000);
        var label;
        if (diffMin < 1) label = 'az once';
        else if (diffMin < 60) label = diffMin + ' dk once';
        else if (diffMin < 1440) label = Math.floor(diffMin / 60) + ' saat once';
        else label = d.toLocaleDateString('tr-TR');
        var prefix = account.lastTestStatus === 'success' ? 'Son test basarili' : 'Son test hatali';
        return prefix + ' - ' + label;
    };

    self.testBadgeClass = function(account) {
        if (!account.lastTestStatus) return 'bg-secondary';
        return account.lastTestStatus === 'success' ? 'bg-success' : 'bg-danger';
    };

    // ─── Load ───

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

        self.loadUsers();
    };

    self.loadUsers = function() {
        $.ajax({ url: API_BASE + '/users', method: 'GET', skipAuthRedirect: true })
            .done(function(data) {
                self.isAdmin(true);
                self.users(data);
            })
            .fail(function(xhr) {
                if (xhr.status === 403 || xhr.status === 401) {
                    self.isAdmin(false);
                } else {
                    toastr.error('Kullanicilar yuklenemedi');
                }
            });
    };

    // ─── User CRUD ───

    self.openUserModal = function() {
        self.isEditingUser(false);
        self.editingUserId = null;
        self.userFirstName('');
        self.userLastName('');
        self.userEmail('');
        self.userPassword('');
        self.userRole('Viewer');
        userModal.show();
    };

    self.openEditUserModal = function(u) {
        self.isEditingUser(true);
        self.editingUserId = u.id;
        self.userFirstName(u.firstName);
        self.userLastName(u.lastName);
        self.userEmail(u.email);
        self.userPassword('');
        self.userRole(u.role);
        userModal.show();
    };

    self.saveUser = function() {
        self.isSavingUser(true);
        var body = {
            firstName: self.userFirstName(),
            lastName: self.userLastName(),
            email: self.userEmail(),
            role: self.userRole()
        };
        var promise;
        if (self.isEditingUser()) {
            promise = apiPut('/users/' + self.editingUserId, body);
        } else {
            body.password = self.userPassword();
            promise = apiPost('/users', body);
        }
        promise
            .done(function() {
                userModal.hide();
                toastr.success(self.isEditingUser() ? 'Kullanici guncellendi' : 'Kullanici olusturuldu');
                self.loadUsers();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Kayit basarisiz')); })
            .always(function() { self.isSavingUser(false); });
    };

    self.openPasswordModal = function(u) {
        self.passwordUserId = u.id;
        self.passwordUserLabel(u.firstName + ' ' + u.lastName + ' (' + u.email + ')');
        self.newPassword('');
        passwordModal.show();
    };

    self.savePassword = function() {
        self.isSavingUser(true);
        apiPut('/users/' + self.passwordUserId + '/password', { newPassword: self.newPassword() })
            .done(function() {
                passwordModal.hide();
                toastr.success('Sifre guncellendi');
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Sifre guncellenemedi')); })
            .always(function() { self.isSavingUser(false); });
    };

    self.deleteUser = function(u) {
        showConfirm(u.firstName + ' ' + u.lastName + ' kullanicisini silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/users/' + u.id)
                .done(function() {
                    toastr.success('Kullanici silindi');
                    self.loadUsers();
                })
                .fail(function(xhr) { toastr.error(extractError(xhr, 'Silinemedi')); });
        });
    };

    // ─── OneDrive Create ───

    self.openCreateModal = function() {
        self.isEditingOd(false);
        self.editingOdId = null;
        self.formName('');
        self.formTenantId('');
        self.formClientId('');
        self.formClientSecret('');
        self.formDriveUserId('');
        self.formDriveId('');
        self.formBasePath('Sertifikalar');
        self.formIsDefault(false);
        self.formIsEnabled(true);
        self.odSetupMode('easy');
        self.odTab('easy');
        self.odConnected(false);
        self.odDrives([]);
        self.odSelectedDrive('');
        self.odRefreshToken('');
        self.odReconnectId(0);
        formModal.show();
    };

    self.openEditOdModal = function(account) {
        self.isEditingOd(true);
        self.editingOdId = account.id;
        self.formName(account.name);
        self.formTenantId(account.tenantId || '');
        self.formDriveId(account.driveId || '');
        self.formDriveUserId(account.driveUserId || '');
        self.formClientId('');
        self.formClientSecret('');
        self.formBasePath(account.basePath || 'Sertifikalar');
        self.formIsDefault(!!account.isDefault);
        self.formIsEnabled(account.isEnabled !== false);
        formModal.show();
    };

    // ─── Quota helpers ───

    self.formatQuota = function(account) {
        if (!account.quotaTotalBytes) return null;
        var used = account.quotaUsedBytes || 0;
        var total = account.quotaTotalBytes;
        var pct = Math.round((used / total) * 1000) / 10;
        var gb = function(b) { return (b / 1073741824).toFixed(2) + ' GB'; };
        return { used: used, total: total, pct: pct, usedLabel: gb(used), totalLabel: gb(total) };
    };

    self.quotaBarClass = function(account) {
        var q = self.formatQuota(account);
        if (!q) return 'bg-secondary';
        if (q.pct > 90) return 'bg-danger';
        if (q.pct > 75) return 'bg-warning';
        return 'bg-success';
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
                toastr.error(extractError(xhr, 'OAuth baslatma hatasi'));
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
                toastr.success('Microsoft hesabi baglandi. Drive secip kaydedin.');
            } else {
                toastr.error(res.error || 'Token alinamadi');
            }
        })
        .fail(function(xhr) { toastr.error(extractError(xhr, 'Token exchange hatasi')); })
        .always(function() { self.odAuthLoading(false); });
    };

    self.reconnect = function() {
        self.odConnected(false);
        self.odDrives([]);
        self.odSelectedDrive('');
        self.odRefreshToken('');
    };

    self.saveOAuthAccount = function() {
        if (!self.formName()) { toastr.warning('Hesap adi girin'); return; }
        if (!self.odSelectedDrive()) { toastr.warning('Drive secin'); return; }

        self.isSaving(true);
        // Reconnect akisi: mevcut hesap icin token tazele
        if (self.odReconnectId() > 0) {
            apiPost('/onedrive-accounts/oauth/reconnect/' + self.odReconnectId(), {
                refreshToken: self.odRefreshToken(),
                driveId: self.odSelectedDrive()
            })
            .done(function() {
                formModal.hide();
                toastr.success('OneDrive hesabi yeniden baglandi.');
                self.loadData();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Yeniden baglanti basarisiz')); })
            .always(function() { self.isSaving(false); });
            return;
        }

        apiPost('/onedrive-accounts/oauth/save', {
            name: self.formName(),
            tenantId: self.formTenantId() || null,
            refreshToken: self.odRefreshToken(),
            driveId: self.odSelectedDrive(),
            basePath: self.formBasePath(),
            isDefault: self.formIsDefault()
        })
        .done(function(res) {
            if (res.success) {
                formModal.hide();
                toastr.success('OneDrive hesabi eklendi.');
                self.loadData();
            } else {
                toastr.error('Hesap kaydedilemedi');
            }
        })
        .fail(function(xhr) { toastr.error(extractError(xhr, 'Hesap kaydedilemedi')); })
        .always(function() { self.isSaving(false); });
    };

    self.saveAccount = function() {
        self.isSaving(true);
        var body = {
            name: self.formName(),
            tenantId: self.formTenantId(),
            clientId: self.formClientId(),
            clientSecret: self.formClientSecret(),
            driveUserId: self.formDriveUserId(),
            driveId: self.formDriveId(),
            basePath: self.formBasePath(),
            isDefault: self.formIsDefault(),
            isEnabled: self.formIsEnabled()
        };

        var promise = self.isEditingOd()
            ? apiPut('/onedrive-accounts/' + self.editingOdId, body)
            : apiPost('/onedrive-accounts', body);

        promise
            .done(function() {
                formModal.hide();
                toastr.success(self.isEditingOd() ? 'Hesap guncellendi' : 'OneDrive hesabi eklendi');
                self.loadData();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Islem basarisiz')); })
            .always(function() { self.isSaving(false); });
    };

    self.openReconnectModal = function(account) {
        self.isEditingOd(false);
        self.editingOdId = null;
        self.formName(account.name);
        self.formTenantId(account.tenantId || '');
        self.formIsDefault(!!account.isDefault);
        self.formIsEnabled(true);
        self.odTab('easy');
        self.odConnected(false);
        self.odDrives([]);
        self.odSelectedDrive('');
        self.odRefreshToken('');
        self.odReconnectId(account.id);
        formModal.show();
    };

    self.setDefault = function(account) {
        apiPost('/onedrive-accounts/' + account.id + '/set-default')
            .done(function() {
                toastr.success('Varsayilan hesap ayarlandi');
                self.loadData();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Islem basarisiz')); });
    };

    self.toggleOdEnabled = function(account) {
        apiPost('/onedrive-accounts/' + account.id + '/toggle-enabled', { enabled: !account.isEnabled })
            .done(function() {
                toastr.success(account.isEnabled ? 'Hesap devre disi birakildi' : 'Hesap etkinlestirildi');
                self.loadData();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Islem basarisiz')); });
    };

    self.testOdAccount = function(account) {
        self.testingAccountId(account.id);
        apiPost('/onedrive-accounts/' + account.id + '/test-connection')
            .done(function(res) {
                if (res.success) toastr.success('Baglanti basarili: ' + account.name);
                else toastr.error('Baglanti hatasi: ' + (res.error || 'Bilinmeyen'));
                self.loadData();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Test basarisiz')); })
            .always(function() { self.testingAccountId(0); });
    };

    self.deleteAccount = function(account) {
        showConfirm('"' + account.name + '" OneDrive hesabini silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/onedrive-accounts/' + account.id)
                .done(function() {
                    toastr.success('Hesap silindi');
                    self.loadData();
                })
                .fail(function(xhr) { toastr.error(extractError(xhr, 'Silinemedi')); });
        });
    };

    // ─── SMTP CRUD ───

    self.openSmtpCreateModal = function() {
        self.isEditingSmtp(false);
        self.editingSmtpId = null;
        self.smtpName('');
        self.smtpHost('');
        self.smtpPort(587);
        self.smtpUsername('');
        self.smtpPassword('');
        self.smtpPasswordHelp('');
        self.smtpFromEmail('');
        self.smtpFromName('');
        self.smtpUseSsl(true);
        self.smtpIsDefault(false);
        self.smtpIsEnabled(true);
        self.smtpAuthMode('basic');
        self.smtpTenantId('');
        self.smtpClientId('');
        self.smtpClientSecret('');
        self.smtpClientSecretHelp('');
        smtpModal.show();
    };

    self.openSmtpEditModal = function(account) {
        self.isEditingSmtp(true);
        self.editingSmtpId = account.id;
        self.smtpName(account.name);
        self.smtpHost(account.host);
        self.smtpPort(account.port);
        self.smtpUsername(account.username);
        self.smtpPassword('');
        self.smtpPasswordHelp(account.hasPassword ? 'Mevcut sifre saklaniyor. Degistirmek icin yeni sifre girin.' : 'Sifre girmeniz gerekiyor.');
        self.smtpFromEmail(account.fromEmail);
        self.smtpFromName(account.fromName || '');
        self.smtpUseSsl(!!account.useSsl);
        self.smtpIsDefault(!!account.isDefault);
        self.smtpIsEnabled(account.isEnabled !== false);
        self.smtpAuthMode(account.authMode || (account.useGraphApi ? 'graph' : (account.useOAuth ? 'oauth' : 'basic')));
        self.smtpTenantId(account.tenantId || '');
        self.smtpClientId(account.clientId || '');
        self.smtpClientSecret('');
        self.smtpClientSecretHelp(account.hasClientSecret ? 'Mevcut client secret saklaniyor. Degistirmek icin yeni deger girin.' : 'Client secret girmeniz gerekiyor.');
        smtpModal.show();
    };

    self.saveSmtpAccount = function() {
        var mode = self.smtpAuthMode();
        if (mode === 'oauth' || mode === 'graph') {
            if (!self.smtpTenantId() || !self.smtpClientId()) {
                toastr.warning('Tenant ID ve Client ID zorunlu');
                return;
            }
            if (!self.isEditingSmtp() && !self.smtpClientSecret()) {
                toastr.warning('Client Secret girin');
                return;
            }
        }

        self.isSaving(true);
        var body = {
            name: self.smtpName(),
            host: self.smtpHost(),
            port: parseInt(self.smtpPort(), 10) || 587,
            username: self.smtpUsername(),
            password: self.smtpPassword() || null,
            fromEmail: self.smtpFromEmail(),
            fromName: self.smtpFromName() || null,
            useSsl: self.smtpUseSsl(),
            useOAuth: mode === 'oauth' || mode === 'graph',
            useGraphApi: mode === 'graph',
            tenantId: self.smtpTenantId() || null,
            clientId: self.smtpClientId() || null,
            clientSecret: self.smtpClientSecret() || null,
            isDefault: self.smtpIsDefault(),
            isEnabled: self.smtpIsEnabled()
        };

        var promise = self.isEditingSmtp()
            ? apiPut('/smtp-accounts/' + self.editingSmtpId, body)
            : apiPost('/smtp-accounts', body);

        promise
            .done(function() {
                smtpModal.hide();
                toastr.success(self.isEditingSmtp() ? 'SMTP hesabi guncellendi' : 'SMTP hesabi eklendi');
                self.loadData();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Islem basarisiz')); })
            .always(function() { self.isSaving(false); });
    };

    self.setSmtpDefault = function(account) {
        apiPost('/smtp-accounts/' + account.id + '/set-default')
            .done(function() {
                toastr.success('Varsayilan SMTP hesabi ayarlandi');
                self.loadData();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Islem basarisiz')); });
    };

    self.toggleSmtpEnabled = function(account) {
        apiPost('/smtp-accounts/' + account.id + '/toggle-enabled', { enabled: !account.isEnabled })
            .done(function() {
                toastr.success(account.isEnabled ? 'Hesap devre disi birakildi' : 'Hesap etkinlestirildi');
                self.loadData();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Islem basarisiz')); });
    };

    self.testSmtpAccount = function(account) {
        self.testingAccountId(account.id);
        apiPost('/smtp-accounts/' + account.id + '/test-connection')
            .done(function(res) {
                if (res.success) toastr.success('Baglanti + kimlik dogrulama basarili: ' + account.name);
                else toastr.error('Baglanti hatasi: ' + (res.error || 'Bilinmeyen'));
                self.loadData();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Test basarisiz')); })
            .always(function() { self.testingAccountId(0); });
    };

    self.openSendTestModal = function(account) {
        self.sendTestAccountId(account.id);
        self.sendTestAccountName(account.name);
        self.sendTestEmail('');
        sendTestModal.show();
    };

    self.sendTest = function() {
        var to = (self.sendTestEmail() || '').trim();
        if (!to) { toastr.warning('Hedef e-posta girin'); return; }
        self.isSaving(true);
        apiPost('/smtp-accounts/' + self.sendTestAccountId() + '/send-test', { toEmail: to })
            .done(function(res) {
                if (res.success) {
                    sendTestModal.hide();
                    toastr.success('Test maili gonderildi: ' + to);
                    self.loadData();
                } else {
                    toastr.error('Gonderilemedi: ' + (res.error || 'Bilinmeyen'));
                }
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Gonderim basarisiz')); })
            .always(function() { self.isSaving(false); });
    };

    self.deleteSmtpAccount = function(account) {
        showConfirm('"' + account.name + '" SMTP hesabini silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/smtp-accounts/' + account.id)
                .done(function() {
                    toastr.success('SMTP hesabi silindi');
                    self.loadData();
                })
                .fail(function(xhr) { toastr.error(extractError(xhr, 'Silinemedi')); });
        });
    };

    $(document).ready(function() {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        smtpModal = new bootstrap.Modal(document.getElementById('smtpModal'));
        userModal = new bootstrap.Modal(document.getElementById('userModal'));
        passwordModal = new bootstrap.Modal(document.getElementById('passwordModal'));
        sendTestModal = new bootstrap.Modal(document.getElementById('sendTestModal'));
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
