// trainingId is set by the Razor view before this script loads
function TrainingDetailViewModel() {
    var self = this;

    self.training = ko.observable(null);
    self.participants = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isGenerating = ko.observable(false);
    self.isEditingParticipant = ko.observable(false);
    self.editingParticipantId = null;

    self.participantFirstName = ko.observable('');
    self.participantLastName = ko.observable('');
    self.participantEmail = ko.observable('');
    self.participantCompanyName = ko.observable('');
    self.contactName = ko.observable('');
    self.contactEmail = ko.observable('');
    self.previewSrc = ko.observable('');

    var participantModal, excelModal, sendContactModal, previewModal;

    // ─── Derived state (workflow gating) ───

    self.totalParticipants = ko.computed(function() { return self.participants().length; });

    self.generatedCount = ko.computed(function() {
        return self.participants().filter(function(p) { return !!p.certificateNumber; }).length;
    });

    self.withEmailCount = ko.computed(function() {
        return self.participants().filter(function(p) {
            return !!p.email && !!p.certificateNumber;
        }).length;
    });

    self.canGenerate = ko.computed(function() {
        return self.totalParticipants() > 0 && !self.isGenerating();
    });

    self.canPreview = ko.computed(function() {
        var t = self.training();
        return t && t.templateId;
    });

    self.canSendEmail = ko.computed(function() {
        return self.withEmailCount() > 0;
    });

    self.canDownloadZip = ko.computed(function() {
        return self.generatedCount() > 0;
    });

    self.allGenerated = ko.computed(function() {
        return self.totalParticipants() > 0 && self.generatedCount() === self.totalParticipants();
    });

    // Stepper steps: 0=participant, 1=generate, 2=distribute
    self.currentStep = ko.computed(function() {
        if (self.totalParticipants() === 0) return 0;
        if (self.generatedCount() === 0) return 1;
        return 2;
    });

    self.stepClass = function(step) {
        var cur = self.currentStep();
        if (step < cur) return 'step-done';
        if (step === cur) return 'step-active';
        return 'step-pending';
    };

    // ─── Formatters ───

    self.formatDate = function(dateStr) { return formatDate(dateStr); };
    self.formatDateRange = function(start, end) { return formatDateRange(start, end); };

    self.getStatusLabel = function(status) {
        return { 0: 'Taslak', 1: 'Hazir', 2: 'Uretildi', 3: 'Dagitildi' }[status] || '';
    };

    self.getStatusClass = function(status) {
        return { 0: 'bg-secondary', 1: 'bg-info', 2: 'bg-success', 3: 'bg-primary' }[status] || 'bg-secondary';
    };

    // ─── Load ───

    self.loadData = function() {
        self.isLoading(true);
        apiGet('/trainings/' + trainingId)
            .done(function(data) {
                self.training(data);
                self.isLoading(false);
                self.loadParticipants();
            })
            .fail(function(xhr) {
                toastr.error('Egitim yuklenemedi: ' + extractError(xhr));
                self.isLoading(false);
            });
    };

    self.loadParticipants = function() {
        apiGet('/trainings/' + trainingId + '/participants')
            .done(function(data) { self.participants(data); })
            .fail(function(xhr) { toastr.error('Katilimcilar yuklenemedi: ' + extractError(xhr)); });
    };

    // ─── Participant CRUD ───

    self.openParticipantModal = function() {
        self.isEditingParticipant(false);
        self.editingParticipantId = null;
        self.participantFirstName('');
        self.participantLastName('');
        self.participantEmail('');
        self.participantCompanyName('');
        participantModal.show();
    };

    self.openEditParticipantModal = function(p) {
        self.isEditingParticipant(true);
        self.editingParticipantId = p.id;
        self.participantFirstName(p.firstName);
        self.participantLastName(p.lastName);
        self.participantEmail(p.email || '');
        self.participantCompanyName(p.companyName || '');
        participantModal.show();
    };

    self.saveParticipant = function() {
        var first = (self.participantFirstName() || '').trim();
        var last = (self.participantLastName() || '').trim();
        if (!first || !last) {
            toastr.warning('Ad ve soyad zorunludur');
            return;
        }
        self.isSaving(true);
        var data = {
            firstName: first,
            lastName: last,
            email: self.participantEmail() || null,
            companyName: self.participantCompanyName() || null
        };

        var promise = self.isEditingParticipant()
            ? apiPut('/trainings/' + trainingId + '/participants/' + self.editingParticipantId, data)
            : apiPost('/trainings/' + trainingId + '/participants', data);

        promise
            .done(function() {
                participantModal.hide();
                toastr.success(self.isEditingParticipant() ? 'Katilimci guncellendi' : 'Katilimci eklendi');
                self.loadParticipants();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Kayit basarisiz')); })
            .always(function() { self.isSaving(false); });
    };

    self.deleteParticipant = function(p) {
        showConfirm('Katilimciyi silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/trainings/' + trainingId + '/participants/' + p.id)
                .done(function() {
                    toastr.success('Katilimci silindi');
                    self.loadParticipants();
                })
                .fail(function(xhr) { toastr.error(extractError(xhr, 'Silinemedi')); });
        });
    };

    // ─── Excel ───

    self.openExcelModal = function() {
        document.getElementById('excel-file').value = '';
        excelModal.show();
    };

    self.uploadExcel = function() {
        var file = document.getElementById('excel-file').files[0];
        if (!file) { toastr.warning('Dosya secin'); return; }

        self.isSaving(true);
        var formData = new FormData();
        formData.append('file', file);

        apiPost('/trainings/' + trainingId + '/participants/import-excel', formData, true)
            .done(function(res) {
                excelModal.hide();
                toastr.success((res && res.message) || 'Katilimcilar yuklendi');
                self.loadParticipants();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Yuklenemedi')); })
            .always(function() { self.isSaving(false); });
    };

    self.downloadExcelTemplate = function() {
        downloadAuthedFile(
            API_BASE + '/trainings/' + trainingId + '/participants/excel-template',
            'katilimci_sablonu.xlsx'
        );
    };

    // ─── Certificate actions ───

    self.generateCertificates = function() {
        if (!self.canGenerate()) {
            toastr.warning('Once en az bir katilimci ekleyin');
            return;
        }

        var longNames = self.participants().filter(function(p) {
            return (p.firstName + ' ' + p.lastName).length > 40;
        });
        var warning = longNames.length > 0
            ? longNames.length + ' katilimcinin ad soyadi 40 karakterden uzun. '
            : '';

        showConfirm(warning + 'Sertifikalar uretilecek. Devam?').then(function(ok) {
            if (!ok) return;
            self.isGenerating(true);
            apiPost('/trainings/' + trainingId + '/generate')
                .done(function(result) {
                    if (result.failed > 0) {
                        toastr.warning('Uretim: ' + result.success + '/' + result.total +
                            ' basarili, ' + result.failed + ' hata');
                    } else {
                        toastr.success('Uretim tamamlandi: ' + result.success + '/' + result.total);
                    }
                    self.loadData();
                })
                .fail(function(xhr) { toastr.error(extractError(xhr, 'Uretim basarisiz')); })
                .always(function() { self.isGenerating(false); });
        });
    };

    self.previewTemplate = function() {
        // Use first participant so the user sees a real name in the preview
        var first = self.participants()[0];
        var url = API_BASE + '/trainings/' + trainingId + '/preview';
        if (first) url += '?participantId=' + first.id;
        self.openPreview(url);
    };

    self.previewParticipant = function(p) {
        // Works before generation too - preview is always live-rendered
        self.openPreview(API_BASE + '/trainings/' + trainingId + '/preview?participantId=' + p.id);
    };

    self.openPreview = function(url) {
        fetch(url, { method: 'GET', credentials: 'same-origin' })
            .then(function(res) {
                if (!res.ok) {
                    return res.text().then(function(t) {
                        var msg = t;
                        try { msg = JSON.parse(t).error || t; } catch (e) {}
                        throw new Error(msg);
                    });
                }
                return res.blob();
            })
            .then(function(blob) {
                var blobUrl = URL.createObjectURL(blob);
                self.previewSrc(blobUrl);
                previewModal.show();
                document.getElementById('previewModal').addEventListener('hidden.bs.modal', function() {
                    URL.revokeObjectURL(blobUrl);
                    self.previewSrc('');
                }, { once: true });
            })
            .catch(function(err) { toastr.error('Onizleme: ' + (err.message || 'Hata')); });
    };

    self.downloadZip = function() {
        if (!self.canDownloadZip()) {
            toastr.warning('Once sertifikalari uretin. ZIP icin en az bir uretilmis sertifika gerekli.');
            return;
        }
        downloadAuthedFile(
            API_BASE + '/trainings/' + trainingId + '/download-zip',
            'sertifikalar_training_' + trainingId + '.zip'
        );
    };

    self.sendCertificates = function() {
        if (!self.canSendEmail()) {
            toastr.warning('Uretilmis sertifikasi olan + e-postasi dolu katilimci yok.');
            return;
        }
        showConfirm(self.withEmailCount() + ' katilimciya e-posta gonderilecek. Devam?').then(function(ok) {
            if (!ok) return;
            apiPost('/trainings/' + trainingId + '/send-certificates')
                .done(function(result) {
                    if (result.failed > 0) {
                        toastr.warning('Gonderim: ' + result.sent + '/' + result.total +
                            ' basarili, ' + result.failed + ' hata');
                    } else {
                        toastr.success('Gonderim tamamlandi: ' + result.sent + '/' + result.total);
                    }
                    self.loadData();
                })
                .fail(function(xhr) { toastr.error(extractError(xhr, 'Gonderim basarisiz')); });
        });
    };

    self.openSendToContactModal = function() {
        if (!self.canDownloadZip()) {
            toastr.warning('Once sertifikalari uretin.');
            return;
        }
        self.contactName('');
        self.contactEmail('');
        sendContactModal.show();
    };

    self.sendToContact = function() {
        self.isSaving(true);
        apiPost('/trainings/' + trainingId + '/send-to-contact', {
            name: self.contactName(),
            email: self.contactEmail()
        })
            .done(function() {
                sendContactModal.hide();
                toastr.success('Sertifikalar gonderildi');
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Gonderilemedi')); })
            .always(function() { self.isSaving(false); });
    };

    $(document).ready(function() {
        participantModal = new bootstrap.Modal(document.getElementById('participantModal'));
        excelModal = new bootstrap.Modal(document.getElementById('excelModal'));
        sendContactModal = new bootstrap.Modal(document.getElementById('sendContactModal'));
        previewModal = new bootstrap.Modal(document.getElementById('previewModal'));
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new TrainingDetailViewModel(), document.getElementById('trainingDetailApp'));
