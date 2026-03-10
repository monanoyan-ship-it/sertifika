function TrainingsViewModel() {
    var self = this;

    self.trainings = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.editingId = null;
    self.searchQuery = ko.observable('');

    self.formName = ko.observable('');
    self.formDisplayName = ko.observable('');
    self.formDescription = ko.observable('');
    self.formTrainingDate = ko.observable('');
    self.formCompanyName = ko.observable('');
    self.formTemplateId = ko.observable(null);
    self.availableTemplates = ko.observableArray([]);

    self.filteredTrainings = ko.computed(function() {
        var q = (self.searchQuery() || '').toLowerCase().trim();
        if (!q) return self.trainings();
        return self.trainings().filter(function(t) {
            return (t.name || '').toLowerCase().indexOf(q) >= 0
                || (t.displayName || '').toLowerCase().indexOf(q) >= 0
                || (t.companyName || '').toLowerCase().indexOf(q) >= 0;
        });
    });

    self.selectedTemplateSignatures = ko.computed(function() {
        var tid = self.formTemplateId();
        if (!tid) return [];
        var tpl = self.availableTemplates().find(function(t) { return t.id === parseInt(tid); });
        if (!tpl || !tpl.templateSignatures) return [];
        return tpl.templateSignatures.sort(function(a, b) { return a.displayOrder - b.displayOrder; });
    });

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
        self.isEditing(false);
        self.editingId = null;
        self.formName('');
        self.formDisplayName('');
        self.formDescription('');
        self.formTrainingDate('');
        self.formCompanyName('');
        self.formTemplateId(null);

        apiGet('/templates')
            .done(function(data) {
                self.availableTemplates(data);
                if (data.length > 0) {
                    self.formTemplateId(data[0].id);
                }
                formModal.show();
            })
            .fail(function() { toastr.error('Sablonlar yuklenemedi'); });
    };

    self.openEditModal = function(training) {
        self.isEditing(true);
        self.editingId = training.id;
        self.formName(training.name || '');
        self.formDisplayName(training.displayName || '');
        self.formDescription(training.description || '');
        self.formTrainingDate(training.trainingDate ? training.trainingDate.substring(0, 10) : '');
        self.formCompanyName(training.companyName || '');

        apiGet('/templates')
            .done(function(data) {
                self.availableTemplates(data);
                self.formTemplateId(training.templateId);
                formModal.show();
            })
            .fail(function() { toastr.error('Sablonlar yuklenemedi'); });
    };

    self.saveTraining = function() {
        self.isSaving(true);
        var body = {
            name: self.formName(),
            displayName: self.formDisplayName() || null,
            description: self.formDescription(),
            trainingDate: self.formTrainingDate() + 'T00:00:00Z',
            companyName: self.formCompanyName(),
            templateId: parseInt(self.formTemplateId())
        };

        var promise;
        if (self.isEditing()) {
            body.id = self.editingId;
            promise = apiPut('/trainings/' + self.editingId, body);
        } else {
            promise = apiPost('/trainings', body);
        }

        promise
            .done(function() {
                formModal.hide();
                toastr.success(self.isEditing() ? 'Egitim guncellendi' : 'Egitim olusturuldu');
                self.loadData();
            })
            .fail(function(xhr) {
                toastr.error('Hata: ' + (xhr.responseText || 'Islem basarisiz'));
            })
            .always(function() {
                self.isSaving(false);
            });
    };

    self.goToDetail = function(training) {
        window.location.href = '/Panel/TrainingDetail/' + training.id;
    };

    self.deleteTraining = function(training) {
        showConfirm('Egitimi silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/trainings/' + training.id)
                .done(function() {
                    toastr.success('Egitim silindi');
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

requireAuth() && ko.applyBindings(new TrainingsViewModel(), document.getElementById('trainingsApp'));
