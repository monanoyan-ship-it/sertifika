// templateId is set by the Razor view before this script loads
function TemplateEditorViewModel() {
    var self = this;

    self.editorTitle = ko.observable(templateId ? 'Sablon Duzenle' : 'Yeni Sablon');
    self.templateName = ko.observable('');
    self.templateDesc = ko.observable('');
    self.orientation = ko.observable('0');
    self.backgroundPreview = ko.observable('');
    self.isSaving = ko.observable(false);

    self.fields = ko.observableArray([]);
    self.selectedField = ko.observable(null);
    self.availableSignatures = ko.observableArray([]);

    var fieldIdCounter = 0;
    var isDragging = false, isResizing = false;
    var dragOffsetX = 0, dragOffsetY = 0;

    self.getFieldTypeLabel = function(type) {
        return { static: 'Sabit Metin', dynamic: 'Dinamik', qrcode: 'QR Kod', signature: 'Imza' }[type] || type;
    };

    self.signatureOptionText = function(sig) {
        return sig.name + ' (' + sig.title + ')';
    };

    self.getSignatureById = function(id) {
        return self.availableSignatures().find(function(s) { return s.id == id; }) || null;
    };

    // Background preview
    $(document).ready(function() {
        document.getElementById('tpl-bg-file').addEventListener('change', function() {
            var file = this.files[0];
            if (file) {
                var reader = new FileReader();
                reader.onload = function(e) { self.backgroundPreview(e.target.result); };
                reader.readAsDataURL(file);
            } else {
                self.backgroundPreview('');
            }
        });

        // Load available signatures
        apiGet('/signatures')
            .done(function(data) {
                self.availableSignatures(data);
                if (templateId) {
                    self.loadTemplate();
                }
            })
            .fail(function() {
                if (templateId) {
                    self.loadTemplate();
                }
            });

        self.orientation.subscribe(function() { self.renderFields(); });
    });

    // Load existing template
    self.loadTemplate = function() {
        apiGet('/templates/' + templateId)
            .done(function(tpl) {
                self.templateName(tpl.name);
                self.templateDesc(tpl.description || '');
                self.orientation(String(tpl.orientation));
                self.editorTitle('Sablon: ' + tpl.name);

                if (tpl.backgroundImageUrl) {
                    self.backgroundPreview(tpl.backgroundImageUrl);
                }

                var parsed = JSON.parse(tpl.layoutJson || '[]').map(function(f, i) {
                    return self.createFieldObj(i, {
                        fieldType: f.fieldType || f.FieldType || 'static',
                        dynamicKey: f.dynamicKey || f.DynamicKey || null,
                        staticText: f.staticText || f.StaticText || '',
                        signatureId: f.signatureId || f.SignatureId || null,
                        x: f.x || f.X || 10,
                        y: f.y || f.Y || 10,
                        width: f.width || f.Width || 30,
                        height: f.height || f.Height || 5,
                        fontFamily: f.fontFamily || f.FontFamily || 'Arial',
                        fontSize: f.fontSize || f.FontSize || 14,
                        fontColor: f.fontColor || f.FontColor || '#000000',
                        isBold: f.isBold || f.IsBold || false,
                        isItalic: f.isItalic || f.IsItalic || false,
                        textAlign: f.textAlign || f.TextAlign || 'center'
                    });
                });
                fieldIdCounter = parsed.length;
                self.fields(parsed);
                self.renderFields();
            })
            .fail(function() { toastr.error('Sablon yuklenemedi'); });
    };

    self.createFieldObj = function(id, data) {
        return {
            id: id,
            fieldType: data.fieldType,
            dynamicKey: data.dynamicKey,
            staticText: data.staticText || '',
            signatureId: data.signatureId || null,
            x: data.x, y: data.y, width: data.width, height: data.height,
            fontFamily: data.fontFamily || 'Arial',
            fontSize: data.fontSize || 14,
            fontColor: data.fontColor || '#000000',
            isBold: data.isBold || false,
            isItalic: data.isItalic || false,
            textAlign: data.textAlign || 'center'
        };
    };

    // Add fields
    self.addStaticField = function() { self.addField('static'); };
    self.addDynamicField = function() { self.addField('dynamic'); };
    self.addQrCodeField = function() { self.addField('qrcode'); };
    self.addSignatureField = function() {
        if (self.availableSignatures().length === 0) {
            toastr.warning('Henuz imza eklenmemis. Once Imzalar sayfasindan imza ekleyin.');
            return;
        }
        self.addField('signature');
    };

    self.addField = function(type) {
        var firstSigId = self.availableSignatures().length > 0 ? self.availableSignatures()[0].id : null;
        var data = {
            fieldType: type,
            dynamicKey: type === 'dynamic' ? 'HolderName' : (type === 'qrcode' ? 'QrCode' : null),
            staticText: type === 'static' ? 'Metin' : '',
            signatureId: type === 'signature' ? firstSigId : null,
            x: 30, y: 30, width: 25, height: 5,
            fontFamily: 'Arial', fontSize: 14, fontColor: '#000000',
            isBold: false, isItalic: false, textAlign: 'center'
        };
        if (type === 'qrcode') {
            data.width = 10; data.height = 10; data.x = 80; data.y = 75;
        }
        if (type === 'signature') {
            data.width = 12; data.height = 8; data.x = 15; data.y = 80;
        }
        var field = self.createFieldObj(fieldIdCounter++, data);
        self.fields.push(field);
        self.selectField(field);
        self.renderFields();
    };

    self.selectField = function(field) {
        self.selectedField(field);
        self.renderFields();
    };

    self.deleteSelectedField = function() {
        var f = self.selectedField();
        if (!f) return;
        self.fields.remove(f);
        self.selectedField(null);
        self.renderFields();
    };

    self.onFieldChanged = function() {
        self.renderFields();
    };

    // Render fields on canvas
    self.renderFields = function() {
        var canvas = document.getElementById('editor-canvas');
        canvas.querySelectorAll('.editor-field').forEach(function(el) { el.remove(); });

        self.fields().forEach(function(field) {
            var el = document.createElement('div');
            el.className = 'editor-field' + (self.selectedField() === field ? ' selected' : '');
            el.style.left = field.x + '%';
            el.style.top = field.y + '%';
            el.style.width = field.width + '%';
            el.style.height = field.height + '%';
            el.style.position = 'absolute';
            el.style.border = self.selectedField() === field ? '2px solid #3498db' : '1px dashed #999';
            el.style.cursor = 'move';
            el.style.display = 'flex';
            el.style.alignItems = 'center';
            el.style.justifyContent = 'center';
            el.style.overflow = 'hidden';
            el.style.backgroundColor = 'rgba(255,255,255,0.7)';
            el.style.borderRadius = '3px';

            var label = document.createElement('span');
            if (field.fieldType === 'signature') {
                var sig = self.getSignatureById(field.signatureId);
                if (sig && sig.imageUrl) {
                    var sigImg = document.createElement('img');
                    sigImg.src = sig.imageUrl;
                    sigImg.style.cssText = 'max-width:100%;max-height:100%;object-fit:contain;pointer-events:none;';
                    el.appendChild(sigImg);
                } else {
                    label.textContent = '[Imza' + (sig ? ': ' + sig.name : '') + ']';
                    label.style.color = '#e67e22';
                    label.style.width = '100%';
                    label.style.textAlign = 'center';
                    label.style.fontSize = '12px';
                    label.style.pointerEvents = 'none';
                    el.appendChild(label);
                }
            } else {
                if (field.fieldType === 'dynamic') {
                    var keyLabels = {
                        HolderName: 'Ad Soyad', TrainingName: 'Egitim Adi', TrainingDate: 'Tarih',
                        CompanyName: 'Firma', CertificateNo: 'Sertifika No', QrCode: 'QR Kod'
                    };
                    label.textContent = '[' + (keyLabels[field.dynamicKey] || field.dynamicKey) + ']';
                    label.style.color = '#3498db';
                } else if (field.fieldType === 'qrcode') {
                    label.textContent = '[QR Kod]';
                    label.style.color = '#27ae60';
                } else {
                    label.textContent = field.staticText || 'Metin';
                }
                label.style.fontFamily = field.fontFamily;
                label.style.fontSize = Math.min(field.fontSize * 0.8, 18) + 'px';
                label.style.fontWeight = field.isBold ? 'bold' : 'normal';
                label.style.fontStyle = field.isItalic ? 'italic' : 'normal';
                label.style.textAlign = field.textAlign;
                label.style.width = '100%';
                label.style.pointerEvents = 'none';
                el.appendChild(label);
            }

            var handle = document.createElement('div');
            handle.style.cssText = 'position:absolute;right:0;bottom:0;width:10px;height:10px;background:#3498db;cursor:se-resize;';
            el.appendChild(handle);

            el.addEventListener('mousedown', function(e) {
                if (e.target === handle) {
                    startResize(e, field);
                } else {
                    startDrag(e, field);
                }
                self.selectField(field);
                e.preventDefault();
            });

            canvas.appendChild(el);
        });
    };

    // Drag & Drop
    function startDrag(e, field) {
        isDragging = true;
        var canvas = document.getElementById('editor-canvas');
        var rect = canvas.getBoundingClientRect();
        dragOffsetX = (e.clientX - rect.left) / rect.width * 100 - field.x;
        dragOffsetY = (e.clientY - rect.top) / rect.height * 100 - field.y;

        var onMove = function(ev) {
            if (!isDragging) return;
            var r = canvas.getBoundingClientRect();
            field.x = Math.max(0, Math.min(100 - field.width, (ev.clientX - r.left) / r.width * 100 - dragOffsetX));
            field.y = Math.max(0, Math.min(100 - field.height, (ev.clientY - r.top) / r.height * 100 - dragOffsetY));
            self.renderFields();
        };
        var onUp = function() {
            isDragging = false;
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup', onUp);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
    }

    function startResize(e, field) {
        isResizing = true;
        var canvas = document.getElementById('editor-canvas');

        var onMove = function(ev) {
            if (!isResizing) return;
            var r = canvas.getBoundingClientRect();
            var newW = (ev.clientX - r.left) / r.width * 100 - field.x;
            var newH = (ev.clientY - r.top) / r.height * 100 - field.y;
            field.width = Math.max(5, Math.min(100 - field.x, newW));
            field.height = Math.max(2, Math.min(100 - field.y, newH));
            self.renderFields();
        };
        var onUp = function() {
            isResizing = false;
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup', onUp);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
    }

    // Save
    self.saveTemplate = function() {
        if (!self.templateName()) { toastr.warning('Sablon adi gerekli'); return; }

        self.isSaving(true);
        var layout = self.fields().map(function(f) {
            return {
                FieldType: f.fieldType, DynamicKey: f.dynamicKey, StaticText: f.staticText,
                SignatureId: f.signatureId,
                X: f.x, Y: f.y, Width: f.width, Height: f.height,
                FontFamily: f.fontFamily, FontSize: f.fontSize, FontColor: f.fontColor,
                IsBold: f.isBold, IsItalic: f.isItalic, TextAlign: f.textAlign
            };
        });

        var body = {
            name: self.templateName(),
            description: self.templateDesc(),
            orientation: parseInt(self.orientation()),
            layoutJson: JSON.stringify(layout)
        };

        var promise;
        if (templateId) {
            body.id = templateId;
            promise = apiPut('/templates/' + templateId, body);
        } else {
            promise = apiPost('/templates', body);
        }

        promise
            .done(function(result) {
                var savedId = templateId || (result && result.id);

                var bgFile = document.getElementById('tpl-bg-file').files[0];
                if (bgFile && savedId) {
                    var formData = new FormData();
                    formData.append('file', bgFile);
                    apiPost('/templates/' + savedId + '/upload-background', formData, true)
                        .done(function() {
                            toastr.success('Sablon kaydedildi');
                            window.location.href = '/Panel/Templates';
                        })
                        .fail(function() {
                            toastr.warning('Sablon kaydedildi ama arka plan yuklenemedi');
                            window.location.href = '/Panel/Templates';
                        });
                } else {
                    toastr.success('Sablon kaydedildi');
                    window.location.href = '/Panel/Templates';
                }

                self.isSaving(false);
            })
            .fail(function(xhr) {
                toastr.error('Hata: ' + (xhr.responseText || 'Kaydedilemedi'));
                self.isSaving(false);
            });
    };
}

if (requireAuth()) {
    ko.applyBindings(new TemplateEditorViewModel(), document.getElementById('editorApp'));
}
