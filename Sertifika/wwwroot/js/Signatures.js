function SignaturesViewModel() {
    var self = this;

    self.signatures = ko.observableArray([]);
    self.searchQuery = ko.observable('');
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);

    self.filteredSignatures = ko.computed(function() {
        var q = self.searchQuery().toLocaleLowerCase('tr');
        if (!q) return self.signatures();
        return self.signatures().filter(function(sig) {
            return (sig.name || '').toLocaleLowerCase('tr').indexOf(q) > -1 ||
                   (sig.title || '').toLocaleLowerCase('tr').indexOf(q) > -1;
        });
    });
    self.editingId = null;
    self.formName = ko.observable('');
    self.formTitle = ko.observable('');

    // Bulk
    self.bulkTitle = ko.observable('');
    self.bulkFiles = ko.observableArray([]);
    self.isBulkSaving = ko.observable(false);

    var createModal, bulkModal;

    function nameFromFileName(fileName) {
        var name = fileName.replace(/\.[^.]+$/, '');
        name = name.replace(/[_\-\.]/g, ' ');
        name = name.replace(/\s+/g, ' ').trim();
        name = name.toLocaleLowerCase('tr');
        return name.replace(/(^|\s)\S/g, function(c) { return c.toLocaleUpperCase('tr'); });
    }

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
        self.isEditing(false);
        self.editingId = null;
        self.formName('');
        self.formTitle('');
        document.getElementById('sig-file').value = '';
        createModal.show();
    };

    self.openEditModal = function(sig) {
        self.isEditing(true);
        self.editingId = sig.id;
        self.formName(sig.name);
        self.formTitle(sig.title);
        document.getElementById('sig-file').value = '';
        createModal.show();
    };

    self.saveSignature = function() {
        var file = document.getElementById('sig-file').files[0];

        if (!self.isEditing() && !file) { toastr.warning('Imza gorseli secin'); return; }

        self.isSaving(true);
        var fd = new FormData();
        fd.append('Name', self.formName());
        fd.append('Title', self.formTitle());
        if (file) fd.append('file', file);

        var promise;
        if (self.isEditing()) {
            promise = apiPut('/signatures/' + self.editingId, fd, true);
        } else {
            promise = apiPost('/signatures', fd, true);
        }

        promise
            .done(function() {
                createModal.hide();
                toastr.success(self.isEditing() ? 'Imza guncellendi' : 'Imza eklendi');
                self.loadData();
            })
            .fail(function() {
                toastr.error(self.isEditing() ? 'Guncellenemedi' : 'Imza eklenemedi');
            })
            .always(function() {
                self.isSaving(false);
            });
    };

    // Bulk operations
    self.openBulkModal = function() {
        self.bulkTitle('');
        self.bulkFiles([]);
        document.getElementById('bulk-files').value = '';
        var folderInput = document.getElementById('bulk-folder');
        if (folderInput) folderInput.value = '';
        bulkModal.show();
    };

    self.handleBulkFiles = function(vm, event) {
        var files = event.target.files;
        if (!files || files.length === 0) return;
        var imageExts = ['.png', '.jpg', '.jpeg'];
        var items = [];
        for (var i = 0; i < files.length; i++) {
            var f = files[i];
            var ext = f.name.substring(f.name.lastIndexOf('.')).toLowerCase();
            if (imageExts.indexOf(ext) === -1) continue;
            items.push({ file: f, name: ko.observable(nameFromFileName(f.name)) });
        }
        var merged = self.bulkFiles().concat(items);
        self.bulkFiles(merged);
    };

    self.removeBulkFile = function(item) {
        self.bulkFiles.remove(item);
    };

    self.clearBulkFiles = function() {
        self.bulkFiles([]);
        document.getElementById('bulk-files').value = '';
        var folderInput = document.getElementById('bulk-folder');
        if (folderInput) folderInput.value = '';
    };

    self.saveBulk = function() {
        if (self.bulkFiles().length === 0) { toastr.warning('Dosya secin'); return; }
        if (!self.bulkTitle().trim()) { toastr.warning('Unvan girin'); return; }

        self.isBulkSaving(true);
        var fd = new FormData();
        fd.append('title', self.bulkTitle().trim());

        var names = [];
        self.bulkFiles().forEach(function(item) {
            fd.append('files', item.file);
            names.push(item.name());
        });
        fd.append('namesJson', JSON.stringify(names));

        apiPost('/signatures/bulk', fd, true)
            .done(function(result) {
                bulkModal.hide();
                toastr.success(result.count + ' imza eklendi');
                self.loadData();
            })
            .fail(function(xhr) {
                var msg = 'Toplu ekleme basarisiz';
                try { msg = JSON.parse(xhr.responseText).message || msg; } catch(e) {}
                toastr.error(msg);
            })
            .always(function() {
                self.isBulkSaving(false);
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
        bulkModal = new bootstrap.Modal(document.getElementById('bulkModal'));
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new SignaturesViewModel(), document.getElementById('signaturesApp'));
