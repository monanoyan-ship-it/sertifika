function SignaturesViewModel() {
    var self = this;

    self.signatures = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.formData = ko.observable({ name: '', title: '' });

    var createModal;

    self.loadData = function() {
        self.isLoading(true);
        apiGet('/signatures')
            .done(function(data) {
                self.signatures(data);
                self.isLoading(false);
            })
            .fail(function() {
                toastr.error('Imzalar yuklenemedi');
                self.isLoading(false);
            });
    };

    self.openCreateModal = function() {
        self.formData({ name: '', title: '' });
        document.getElementById('sig-file').value = '';
        createModal.show();
    };

    self.saveSignature = function() {
        var file = document.getElementById('sig-file').files[0];
        if (!file) { toastr.warning('Imza gorseli secin'); return; }

        self.isSaving(true);
        var formData = new FormData();
        formData.append('Name', self.formData().name);
        formData.append('Title', self.formData().title);
        formData.append('file', file);

        apiPost('/signatures', formData, true)
            .done(function() {
                createModal.hide();
                toastr.success('Imza eklendi');
                self.loadData();
                self.isSaving(false);
            })
            .fail(function() {
                toastr.error('Imza eklenemedi');
                self.isSaving(false);
            });
    };

    self.deleteSignature = function(sig) {
        showConfirm('Imzayi silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/signatures/' + sig.id)
                .done(function() {
                    toastr.success('Imza silindi');
                    self.loadData();
                })
                .fail(function() { toastr.error('Silinemedi'); });
        });
    };

    $(document).ready(function() {
        createModal = new bootstrap.Modal(document.getElementById('createModal'));
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new SignaturesViewModel(), document.getElementById('signaturesApp'));
