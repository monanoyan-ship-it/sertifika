function TemplatesViewModel() {
    var self = this;

    self.templates = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.editingTemplateId = null;
    self.editingTemplateName = ko.observable('');

    self.signatureRows = ko.observableArray([]);

    var sigModal;

    function makeSignatureRow(sig, existing) {
        return {
            id: sig.id,
            name: sig.name,
            title: sig.title,
            selected: ko.observable(!!existing),
            instructorName: ko.observable(existing ? (existing.instructorName || sig.name) : sig.name),
            instructorTitle: ko.observable(existing ? (existing.instructorTitle || sig.title) : sig.title),
            // Preserve values set in TemplateEditor
            _showName: existing ? existing.showName : true,
            _showTitle: existing ? existing.showTitle : true,
            _imageX: existing ? existing.imageX : 0,
            _imageY: existing ? existing.imageY : 80,
            _imageWidth: existing ? existing.imageWidth : 12,
            _imageHeight: existing ? existing.imageHeight : 8,
            _nameX: existing ? existing.nameX : 0,
            _nameY: existing ? existing.nameY : 90,
            _titleX: existing ? existing.titleX : 0,
            _titleY: existing ? existing.titleY : 93
        };
    }

    self.loadData = function() {
        self.isLoading(true);
        apiGet('/templates')
            .done(function(data) {
                self.templates(data);
                self.isLoading(false);
            })
            .fail(function() {
                toastr.error('Sablonlar yuklenemedi');
                self.isLoading(false);
            });
    };

    self.openSignatureModal = function(tpl) {
        self.editingTemplateId = tpl.id;
        self.editingTemplateName(tpl.name);

        var existingMap = {};
        (tpl.templateSignatures || []).forEach(function(ts) {
            existingMap[ts.signatureId] = ts;
        });

        apiGet('/signatures')
            .done(function(sigs) {
                self.signatureRows(sigs.map(function(sig) {
                    return makeSignatureRow(sig, existingMap[sig.id]);
                }));
                sigModal.show();
            })
            .fail(function() { toastr.error('Imzalar yuklenemedi'); });
    };

    self.saveSignatures = function() {
        self.isSaving(true);
        var signatures = [];
        self.signatureRows().forEach(function(row) {
            if (row.selected()) {
                signatures.push({
                    signatureId: row.id,
                    instructorName: row.instructorName(),
                    instructorTitle: row.instructorTitle(),
                    showName: row._showName,
                    showTitle: row._showTitle,
                    imageX: row._imageX,
                    imageY: row._imageY,
                    imageWidth: row._imageWidth,
                    imageHeight: row._imageHeight,
                    nameX: row._nameX,
                    nameY: row._nameY,
                    titleX: row._titleX,
                    titleY: row._titleY
                });
            }
        });
        apiPut('/templates/' + self.editingTemplateId + '/signatures', { signatures: signatures })
            .done(function() {
                sigModal.hide();
                toastr.success('Imzalar guncellendi');
                self.loadData();
            })
            .fail(function() { toastr.error('Islem basarisiz'); })
            .always(function() { self.isSaving(false); });
    };

    self.deleteTemplate = function(tpl) {
        showConfirm('Sablonu silmek istiyor musunuz?').then(function(ok) {
            if (!ok) return;
            apiDelete('/templates/' + tpl.id)
                .done(function() {
                    toastr.success('Sablon silindi');
                    self.loadData();
                })
                .fail(function() { toastr.error('Silinemedi'); });
        });
    };

    $(document).ready(function() {
        sigModal = new bootstrap.Modal(document.getElementById('signatureModal'));
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new TemplatesViewModel(), document.getElementById('templatesApp'));
