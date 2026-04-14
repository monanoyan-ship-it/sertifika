// trainingId is set by the Razor view before this script loads
function TrainingDetailViewModel() {
    var self = this;

    self.training = ko.observable(null);
    self.participants = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isEditingParticipant = ko.observable(false);
    self.editingParticipantId = null;

    self.participantFirstName = ko.observable('');
    self.participantLastName = ko.observable('');
    self.participantEmail = ko.observable('');
    self.participantCompanyName = ko.observable('');
    self.contactName = ko.observable('');
    self.contactEmail = ko.observable('');

    var participantModal, excelModal, sendContactModal;

    self.formatDate = function(dateStr) { return formatDate(dateStr); };
    self.formatDateRange = function(start, end) { return formatDateRange(start, end); };

    self.getStatusLabel = function(status) {
        return { 0: 'Taslak', 1: 'Hazir', 2: 'Uretildi', 3: 'Dagitildi' }[status] || '';
    };

    self.getStatusClass = function(status) {
        return { 0: 'bg-secondary', 1: 'bg-info', 2: 'bg-success', 3: 'bg-primary' }[status] || 'bg-secondary';
    };

    self.loadData = function() {
        self.isLoading(true);
        apiGet('/trainings/' + trainingId)
            .done(function(data) {
                self.training(data);
                self.isLoading(false);
                self.loadParticipants();
            })
            .fail(function() {
                toastr.error('Egitim yuklenemedi');
                self.isLoading(false);
            });
    };

    self.loadParticipants = function() {
        apiGet('/trainings/' + trainingId + '/participants')
            .done(function(data) { self.participants(data); })
            .fail(function() { console.error('Katilimcilar yuklenemedi'); });
    };

    // Participant CRUD
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
        self.isSaving(true);
        var data = {
            firstName: self.participantFirstName(),
            lastName: self.participantLastName(),
            email: self.participantEmail(),
            companyName: self.participantCompanyName(),
            trainingId: parseInt(trainingId)
        };

        var promise;
        if (self.isEditingParticipant()) {
            data.id = self.editingParticipantId;
            promise = apiPut('/trainings/' + trainingId + '/participants/' + self.editingParticipantId, data);
        } else {
            promise = apiPost('/trainings/' + trainingId + '/participants', data);
        }

        promise
            .done(function() {
                participantModal.hide();
                toastr.success(self.isEditingParticipant() ? 'Katilimci guncellendi' : 'Katilimci eklendi');
                self.loadParticipants();
            })
            .fail(function(xhr) {
                toastr.error('Hata: ' + (xhr.responseText || 'Islem basarisiz'));
            })
            .always(function() {
                self.isSaving(false);
            });
    };

    self.deleteParticipant = function(p) {
        showConfirm('Katilimciyi silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/trainings/' + trainingId + '/participants/' + p.id)
                .done(function() {
                    toastr.success('Katilimci silindi');
                    self.loadParticipants();
                })
                .fail(function() { toastr.error('Silinemedi'); });
        });
    };

    // Excel
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
            .done(function() {
                excelModal.hide();
                toastr.success('Katilimcilar yuklendi');
                self.loadParticipants();
            })
            .fail(function(xhr) {
                toastr.error('Hata: ' + (xhr.responseText || 'Yuklenemedi'));
            })
            .always(function() {
                self.isSaving(false);
            });
    };

    // Certificate actions
    self.generateCertificates = function() {
        var longNames = self.participants().filter(function(p) {
            return (p.firstName + ' ' + p.lastName).length > 40;
        });
        if (longNames.length > 0) {
            toastr.warning(longNames.length + ' katilimcinin ad soyadi 40 karakterden uzun.');
        }

        showConfirm('Sertifikalar uretilecek. Devam?').then(function(ok) {
            if (!ok) return;
            apiPost('/trainings/' + trainingId + '/generate')
                .done(function(result) {
                    toastr.success('Uretim: ' + result.success + '/' + result.total + ' basarili');
                    self.loadData();
                })
                .fail(function(xhr) {
                    toastr.error('Hata: ' + (xhr.responseText || 'Uretim basarisiz'));
                });
        });
    };

    self.previewCertificate = function() {
        window.open(API_BASE + '/trainings/' + trainingId + '/preview', '_blank');
    };

    self.downloadZip = function() {
        downloadAuthedFile(
            API_BASE + '/trainings/' + trainingId + '/download-zip',
            'certificates_training_' + trainingId + '.zip'
        );
    };

    self.downloadExcelTemplate = function() {
        downloadAuthedFile(
            API_BASE + '/trainings/' + trainingId + '/participants/excel-template',
            'katilimci_sablonu.xlsx'
        );
    };

    self.sendCertificates = function() {
        showConfirm('Sertifikalar e-posta ile gonderilecek. Devam?').then(function(ok) {
            if (!ok) return;
            apiPost('/trainings/' + trainingId + '/send-certificates')
                .done(function(result) {
                    toastr.success('Gonderim: ' + result.sent + '/' + result.total + ' basarili');
                    self.loadData();
                })
                .fail(function(xhr) {
                    toastr.error('Hata: ' + (xhr.responseText || 'Gonderim basarisiz'));
                });
        });
    };

    self.openSendToContactModal = function() {
        self.contactName('');
        self.contactEmail('');
        sendContactModal.show();
    };

    self.sendToContact = function() {
        self.isSaving(true);
        var body = {
            name: self.contactName(),
            email: self.contactEmail()
        };
        apiPost('/trainings/' + trainingId + '/send-to-contact', body)
            .done(function() {
                sendContactModal.hide();
                toastr.success('Sertifikalar gonderildi');
            })
            .fail(function(xhr) {
                toastr.error('Hata: ' + (xhr.responseText || 'Gonderilemedi'));
            })
            .always(function() {
                self.isSaving(false);
            });
    };

$(document).ready(function() {
        participantModal = new bootstrap.Modal(document.getElementById('participantModal'));
        excelModal = new bootstrap.Modal(document.getElementById('excelModal'));
        sendContactModal = new bootstrap.Modal(document.getElementById('sendContactModal'));
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new TrainingDetailViewModel(), document.getElementById('trainingDetailApp'));
