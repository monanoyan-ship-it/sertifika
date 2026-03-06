function TrainingsViewModel() {
    var self = this;

    self.trainings = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);

    self.formData = ko.observable({ name: '', description: '', trainingDate: '', companyName: '', templateId: null });
    self.availableTemplates = ko.observableArray([]);
    self.availableSignatures = ko.observableArray([]);
    self.selectedSignatureIds = ko.observableArray([]);

    var formModal;

    self.formatDate = function(dateStr) { return formatDate(dateStr); };

    self.getStatusLabel = function(status) {
        return { 0: 'Taslak', 1: 'Hazir', 2: 'Uretildi', 3: 'Dagitildi' }[status] || '';
    };

    self.getStatusClass = function(status) {
        return { 0: 'bg-secondary', 1: 'bg-info', 2: 'bg-success', 3: 'bg-primary' }[status] || 'bg-secondary';
    };

    self.loadData = function() {
        self.isLoading(true);
        apiGet('/trainings')
            .done(function(data) {
                self.trainings(data);
                self.isLoading(false);
            })
            .fail(function() {
                toastr.error('Egitimler yuklenemedi');
                self.isLoading(false);
            });
    };

    self.openCreateModal = function() {
        self.formData({ name: '', description: '', trainingDate: '', companyName: '', templateId: null });
        self.selectedSignatureIds([]);

        $.when(apiGet('/templates'), apiGet('/signatures'))
            .done(function(tplRes, sigRes) {
                self.availableTemplates(tplRes[0]);
                self.availableSignatures(sigRes[0]);
                if (tplRes[0].length > 0) {
                    self.formData().templateId = tplRes[0][0].id;
                }
                formModal.show();
            })
            .fail(function() { toastr.error('Veriler yuklenemedi'); });
    };

    self.saveTraining = function() {
        self.isSaving(true);
        var data = self.formData();
        var body = {
            name: data.name,
            description: data.description,
            trainingDate: data.trainingDate + 'T00:00:00Z',
            companyName: data.companyName,
            templateId: parseInt(data.templateId),
            signatureIds: self.selectedSignatureIds().map(function(id) { return parseInt(id); })
        };

        apiPost('/trainings', body)
            .done(function() {
                formModal.hide();
                toastr.success('Egitim olusturuldu');
                self.loadData();
                self.isSaving(false);
            })
            .fail(function(xhr) {
                toastr.error('Hata: ' + (xhr.responseText || 'Egitim olusturulamadi'));
                self.isSaving(false);
            });
    };

    $(document).ready(function() {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new TrainingsViewModel(), document.getElementById('trainingsApp'));
