// templateId is set by the Razor view before this script loads
function TemplateEditorViewModel() {
    var self = this;

    self.editorTitle = ko.observable(templateId ? 'Sablon Duzenle' : 'Yeni Sablon');
    self.templateName = ko.observable('');
    self.templateDesc = ko.observable('');
    self.orientation = ko.observable('0');
    self.backgroundPreview = ko.observable('');
    self.backgroundImageUrl = null; // DB'deki mevcut URL
    self.isSaving = ko.observable(false);
    self.isPreviewing = ko.observable(false);

    self.fields = ko.observableArray([]);
    self.templateSignatures = ko.observableArray([]);
    self.availableSignatures = ko.observableArray([]);

    // Signatures not yet added to this template
    self.addableSignatures = ko.computed(function() {
        var usedIds = self.templateSignatures().map(function(ts) { return ts.signatureId; });
        return self.availableSignatures().filter(function(s) {
            return usedIds.indexOf(s.id) === -1;
        });
    });

    // Selection: { kind: 'field'|'sig-image'|'sig-name'|'sig-title', data: object }
    self.selectedItem = ko.observable(null);

    self.selectedField = ko.computed(function() {
        var sel = self.selectedItem();
        return (sel && sel.kind === 'field') ? sel.data : null;
    });

    self.selectedSig = ko.computed(function() {
        var sel = self.selectedItem();
        return (sel && sel.kind && sel.kind.indexOf('sig-') === 0) ? sel.data : null;
    });

    self.selectedSigPart = ko.computed(function() {
        var sel = self.selectedItem();
        return (sel && sel.kind && sel.kind.indexOf('sig-') === 0) ? sel.kind : null;
    });

    var fieldIdCounter = 0;
    var isDragging = false, isResizing = false;
    var dragOffsetX = 0, dragOffsetY = 0;

    self.getFieldTypeLabel = function(type) {
        return { static: 'Sabit Metin', dynamic: 'Dinamik', qrcode: 'QR Kod' }[type] || type;
    };

    function makeSigObservable(ts) {
        var sig = ts.signature || {};
        var obs = {
            signatureId: ts.signatureId,
            signature: sig,
            instructorName: ko.observable(ts.instructorName || sig.name || ''),
            instructorTitle: ko.observable(ts.instructorTitle || sig.title || ''),
            showName: ko.observable(ts.showName !== false),
            showTitle: ko.observable(ts.showTitle !== false),
            imageX: ko.observable(ts.imageX || 0),
            imageY: ko.observable(ts.imageY || 80),
            imageWidth: ko.observable(ts.imageWidth || 12),
            imageHeight: ko.observable(ts.imageHeight || 8),
            imageRotation: ko.observable(ts.imageRotation || 0),
            nameX: ko.observable(ts.nameX || 0),
            nameY: ko.observable(ts.nameY || 90),
            titleX: ko.observable(ts.titleX || 0),
            titleY: ko.observable(ts.titleY || 93),
            nameFontSize: ko.observable(ts.nameFontSize || 8),
            titleFontSize: ko.observable(ts.titleFontSize || 7)
        };
        obs.instructorName.subscribe(function() { self.renderAll(); });
        obs.instructorTitle.subscribe(function() { self.renderAll(); });
        obs.showName.subscribe(function() { self.renderAll(); });
        obs.showTitle.subscribe(function() { self.renderAll(); });
        obs.imageRotation.subscribe(function() { self.renderAll(); });
        obs.nameFontSize.subscribe(function() { self.renderAll(); });
        obs.titleFontSize.subscribe(function() { self.renderAll(); });
        return obs;
    }

    // ==================== INIT ====================

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

        // Load available signatures first, then template
        apiGet('/signatures')
            .done(function(data) { self.availableSignatures(data); })
            .always(function() {
                if (templateId) self.loadTemplate();
            });

        self.orientation.subscribe(function() { self.renderAll(); });
    });

    // ==================== LOAD ====================

    self.loadTemplate = function() {
        apiGet('/templates/' + templateId)
            .done(function(tpl) {
                self.templateName(tpl.name);
                self.templateDesc(tpl.description || '');
                self.orientation(String(tpl.orientation));
                self.editorTitle('Sablon: ' + tpl.name);

                if (tpl.backgroundImageUrl) {
                    self.backgroundImageUrl = tpl.backgroundImageUrl;
                    self.backgroundPreview(tpl.backgroundImageUrl);
                }

                // Parse layout fields — filter out old 'signature' type
                var parsed = JSON.parse(tpl.layoutJson || '[]')
                    .filter(function(f) { return (f.fieldType || f.FieldType || 'static') !== 'signature'; })
                    .map(function(f, i) {
                        return self.createFieldObj(i, {
                            fieldType: f.fieldType || f.FieldType || 'static',
                            dynamicKey: f.dynamicKey || f.DynamicKey || null,
                            staticText: f.staticText || f.StaticText || '',
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

                // Load template signatures
                var sigs = (tpl.templateSignatures || []).map(function(ts) {
                    return makeSigObservable(ts);
                });
                self.templateSignatures(sigs);

                self.renderAll();
            })
            .fail(function() { toastr.error('Sablon yuklenemedi'); });
    };

    self.createFieldObj = function(id, data) {
        return {
            id: id,
            fieldType: data.fieldType,
            dynamicKey: data.dynamicKey,
            staticText: data.staticText || '',
            x: data.x, y: data.y, width: data.width, height: data.height,
            fontFamily: data.fontFamily || 'Arial',
            fontSize: parseFloat(data.fontSize) || 14,
            fontColor: data.fontColor || '#000000',
            isBold: data.isBold || false,
            isItalic: data.isItalic || false,
            textAlign: data.textAlign || 'center'
        };
    };

    // ==================== ADD FIELDS ====================

    self.addStaticField = function() { self.addField('static'); };
    self.addDynamicField = function() { self.addField('dynamic'); };
    self.addQrCodeField = function() { self.addField('qrcode'); };

    self.addField = function(type) {
        var data = {
            fieldType: type,
            dynamicKey: type === 'dynamic' ? 'HolderName' : (type === 'qrcode' ? 'QrCode' : null),
            staticText: type === 'static' ? 'Metin' : '',
            x: 30, y: 30, width: 25, height: 5,
            fontFamily: 'Arial', fontSize: 14, fontColor: '#000000',
            isBold: false, isItalic: false, textAlign: 'center'
        };
        if (type === 'qrcode') {
            data.width = 10; data.height = 10; data.x = 80; data.y = 75;
        }
        var field = self.createFieldObj(fieldIdCounter++, data);
        self.fields.push(field);
        self.selectItem({ kind: 'field', data: field });
    };

    // ==================== ADD / REMOVE SIGNATURE ====================

    self.addSignature = function(sig) {
        var ts = makeSigObservable({
            signatureId: sig.id,
            signature: sig,
            instructorName: sig.name,
            instructorTitle: sig.title,
            showName: true,
            showTitle: true,
            imageX: 0, imageY: 80, imageWidth: 12, imageHeight: 8, imageRotation: 0,
            nameX: 0, nameY: 90,
            titleX: 0, titleY: 93
        });
        self.templateSignatures.push(ts);
        self.selectItem({ kind: 'sig-image', data: ts });
    };

    self.rotateSelectedSignature = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind.indexOf('sig-') !== 0) return;
        var current = sel.data.imageRotation();
        sel.data.imageRotation((current + 1) % 4);
    };

    self.removeSelectedSignature = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind.indexOf('sig-') !== 0) return;
        self.templateSignatures.remove(sel.data);
        self.selectedItem(null);
        self.renderAll();
    };

    // ==================== SELECTION ====================

    self.selectItem = function(item) {
        self.selectedItem(item);
        self.renderAll();
    };

    self.clearSelection = function() {
        self.selectedItem(null);
        self.renderAll();
    };

    self.deleteSelectedField = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind !== 'field') return;
        self.fields.remove(sel.data);
        self.selectedItem(null);
        self.renderAll();
    };

    self.onFieldChanged = function() {
        self.renderAll();
    };

    // ==================== RENDER ====================

    self.renderAll = function() {
        var canvas = document.getElementById('editor-canvas');
        canvas.querySelectorAll('.editor-el').forEach(function(el) { el.remove(); });

        var sel = self.selectedItem();

        self.fields().forEach(function(field) {
            var isSelected = sel && sel.kind === 'field' && sel.data === field;
            renderLayoutField(canvas, field, isSelected);
        });

        self.templateSignatures().forEach(function(sig) {
            renderSigImage(canvas, sig, sel && sel.kind === 'sig-image' && sel.data === sig);
            if (sig.showName()) renderSigName(canvas, sig, sel && sel.kind === 'sig-name' && sel.data === sig);
            if (sig.showTitle()) renderSigTitle(canvas, sig, sel && sel.kind === 'sig-title' && sel.data === sig);
        });
    };

    function createCanvasEl(isSelected, borderColor) {
        var el = document.createElement('div');
        el.className = 'editor-el';
        el.style.position = 'absolute';
        el.style.border = isSelected ? '2px solid ' + borderColor : '1px dashed ' + borderColor;
        el.style.cursor = 'move';
        el.style.display = 'flex';
        el.style.alignItems = 'center';
        el.style.justifyContent = 'center';
        el.style.overflow = 'hidden';
        el.style.borderRadius = '3px';
        el.style.zIndex = isSelected ? '10' : '1';
        return el;
    }

    function renderLayoutField(canvas, field, isSelected) {
        var el = createCanvasEl(isSelected, '#3498db');
        el.style.left = field.x + '%';
        el.style.top = field.y + '%';
        el.style.width = field.width + '%';
        el.style.height = field.height + '%';
        el.style.backgroundColor = 'rgba(255,255,255,0.7)';

        var label = document.createElement('span');
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

        var handle = document.createElement('div');
        handle.style.cssText = 'position:absolute;right:0;bottom:0;width:10px;height:10px;background:#3498db;cursor:se-resize;';
        el.appendChild(handle);

        el.addEventListener('mousedown', function(e) {
            if (e.target === handle) {
                startFieldResize(e, field);
            } else {
                startFieldDrag(e, field);
            }
            self.selectItem({ kind: 'field', data: field });
            e.preventDefault();
        });

        canvas.appendChild(el);
    }

    function renderSigImage(canvas, sig, isSelected) {
        var el = createCanvasEl(isSelected, '#e67e22');
        el.style.left = sig.imageX() + '%';
        el.style.top = sig.imageY() + '%';
        el.style.width = sig.imageWidth() + '%';
        el.style.height = sig.imageHeight() + '%';
        el.style.backgroundColor = 'rgba(230,126,34,0.1)';

        var rot = (sig.imageRotation() || 0) * 90;
        if (sig.signature && sig.signature.imageUrl) {
            var img = document.createElement('img');
            img.src = sig.signature.imageUrl;
            img.style.cssText = 'max-width:100%;max-height:100%;object-fit:contain;pointer-events:none;transform:rotate(' + rot + 'deg);';
            el.appendChild(img);
        } else {
            var lbl = document.createElement('span');
            lbl.textContent = '[Imza: ' + (sig.signature.name || '') + ']';
            lbl.style.cssText = 'color:#e67e22;font-size:10px;pointer-events:none;';
            el.appendChild(lbl);
        }

        var handle = document.createElement('div');
        handle.style.cssText = 'position:absolute;right:0;bottom:0;width:10px;height:10px;background:#e67e22;cursor:se-resize;';
        el.appendChild(handle);

        el.addEventListener('mousedown', function(e) {
            if (e.target === handle) {
                startSigResize(e, sig);
            } else {
                startSigImageDrag(e, sig);
            }
            self.selectItem({ kind: 'sig-image', data: sig });
            e.preventDefault();
        });

        canvas.appendChild(el);
    }

    function renderSigName(canvas, sig, isSelected) {
        var el = createCanvasEl(isSelected, '#27ae60');
        el.style.left = sig.nameX() + '%';
        el.style.top = sig.nameY() + '%';
        el.style.minWidth = '8%';
        el.style.height = '3%';
        el.style.backgroundColor = 'rgba(39,174,96,0.1)';
        el.style.padding = '0 4px';
        el.style.justifyContent = 'flex-start';
        el.style.whiteSpace = 'nowrap';

        var lbl = document.createElement('span');
        lbl.textContent = sig.instructorName() || sig.signature.name || '';
        var nameFsDisplay = Math.min(Math.max(parseInt(sig.nameFontSize()) || 8, 6), 24);
        lbl.style.cssText = 'color:#27ae60;font-size:' + nameFsDisplay + 'px;pointer-events:none;white-space:nowrap;';
        el.appendChild(lbl);

        el.addEventListener('mousedown', function(e) {
            startSigTextDrag(e, sig, 'name');
            self.selectItem({ kind: 'sig-name', data: sig });
            e.preventDefault();
        });

        canvas.appendChild(el);
    }

    function renderSigTitle(canvas, sig, isSelected) {
        var el = createCanvasEl(isSelected, '#8e44ad');
        el.style.left = sig.titleX() + '%';
        el.style.top = sig.titleY() + '%';
        el.style.minWidth = '8%';
        el.style.height = '3%';
        el.style.backgroundColor = 'rgba(142,68,173,0.1)';
        el.style.padding = '0 4px';
        el.style.justifyContent = 'flex-start';
        el.style.whiteSpace = 'nowrap';

        var lbl = document.createElement('span');
        lbl.textContent = sig.instructorTitle() || sig.signature.title || '';
        var titleFsDisplay = Math.min(Math.max(parseInt(sig.titleFontSize()) || 7, 6), 24);
        lbl.style.cssText = 'color:#8e44ad;font-size:' + titleFsDisplay + 'px;pointer-events:none;white-space:nowrap;';
        el.appendChild(lbl);

        el.addEventListener('mousedown', function(e) {
            startSigTextDrag(e, sig, 'title');
            self.selectItem({ kind: 'sig-title', data: sig });
            e.preventDefault();
        });

        canvas.appendChild(el);
    }

    // ==================== DRAG & DROP ====================

    function startFieldDrag(e, field) {
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
            self.renderAll();
        };
        var onUp = function() {
            isDragging = false;
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup', onUp);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
    }

    function startFieldResize(e, field) {
        isResizing = true;
        var canvas = document.getElementById('editor-canvas');
        var onMove = function(ev) {
            if (!isResizing) return;
            var r = canvas.getBoundingClientRect();
            field.width = Math.max(5, Math.min(100 - field.x, (ev.clientX - r.left) / r.width * 100 - field.x));
            field.height = Math.max(2, Math.min(100 - field.y, (ev.clientY - r.top) / r.height * 100 - field.y));
            self.renderAll();
        };
        var onUp = function() {
            isResizing = false;
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup', onUp);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
    }

    function startSigImageDrag(e, sig) {
        isDragging = true;
        var canvas = document.getElementById('editor-canvas');
        var rect = canvas.getBoundingClientRect();
        dragOffsetX = (e.clientX - rect.left) / rect.width * 100 - sig.imageX();
        dragOffsetY = (e.clientY - rect.top) / rect.height * 100 - sig.imageY();

        var onMove = function(ev) {
            if (!isDragging) return;
            var r = canvas.getBoundingClientRect();
            sig.imageX(Math.max(0, Math.min(100 - sig.imageWidth(), (ev.clientX - r.left) / r.width * 100 - dragOffsetX)));
            sig.imageY(Math.max(0, Math.min(100 - sig.imageHeight(), (ev.clientY - r.top) / r.height * 100 - dragOffsetY)));
            self.renderAll();
        };
        var onUp = function() {
            isDragging = false;
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup', onUp);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
    }

    function startSigResize(e, sig) {
        isResizing = true;
        var canvas = document.getElementById('editor-canvas');
        var onMove = function(ev) {
            if (!isResizing) return;
            var r = canvas.getBoundingClientRect();
            sig.imageWidth(Math.max(3, Math.min(100 - sig.imageX(), (ev.clientX - r.left) / r.width * 100 - sig.imageX())));
            sig.imageHeight(Math.max(2, Math.min(100 - sig.imageY(), (ev.clientY - r.top) / r.height * 100 - sig.imageY())));
            self.renderAll();
        };
        var onUp = function() {
            isResizing = false;
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup', onUp);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
    }

    function startSigTextDrag(e, sig, which) {
        isDragging = true;
        var canvas = document.getElementById('editor-canvas');
        var rect = canvas.getBoundingClientRect();
        var xObs = which === 'name' ? sig.nameX : sig.titleX;
        var yObs = which === 'name' ? sig.nameY : sig.titleY;
        dragOffsetX = (e.clientX - rect.left) / rect.width * 100 - xObs();
        dragOffsetY = (e.clientY - rect.top) / rect.height * 100 - yObs();

        var onMove = function(ev) {
            if (!isDragging) return;
            var r = canvas.getBoundingClientRect();
            xObs(Math.max(0, Math.min(92, (ev.clientX - r.left) / r.width * 100 - dragOffsetX)));
            yObs(Math.max(0, Math.min(97, (ev.clientY - r.top) / r.height * 100 - dragOffsetY)));
            self.renderAll();
        };
        var onUp = function() {
            isDragging = false;
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup', onUp);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
    }

    // ==================== PREVIEW ====================

    self.previewTemplate = function() {
        if (!templateId) { toastr.warning('Once sablonu kaydedin'); return; }

        self.isPreviewing(true);

        var layout = self.fields().map(function(f) {
            return {
                FieldType: f.fieldType, DynamicKey: f.dynamicKey, StaticText: f.staticText,
                X: f.x, Y: f.y, Width: f.width, Height: f.height,
                FontFamily: f.fontFamily, FontSize: parseFloat(f.fontSize) || 14, FontColor: f.fontColor,
                IsBold: f.isBold, IsItalic: f.isItalic, TextAlign: f.textAlign
            };
        });

        var sigs = self.templateSignatures().map(function(s) {
            return {
                signatureId: s.signatureId,
                instructorName: s.instructorName(),
                instructorTitle: s.instructorTitle(),
                showName: s.showName(),
                showTitle: s.showTitle(),
                imageX: parseFloat(s.imageX()) || 0,
                imageY: parseFloat(s.imageY()) || 0,
                imageWidth: parseFloat(s.imageWidth()) || 12,
                imageHeight: parseFloat(s.imageHeight()) || 8,
                imageRotation: parseInt(s.imageRotation()) || 0,
                nameX: parseFloat(s.nameX()) || 0,
                nameY: parseFloat(s.nameY()) || 0,
                titleX: parseFloat(s.titleX()) || 0,
                titleY: parseFloat(s.titleY()) || 0,
                nameFontSize: parseInt(s.nameFontSize()) || 8,
                titleFontSize: parseInt(s.titleFontSize()) || 7
            };
        });

        var body = {
            layoutJson: JSON.stringify(layout),
            orientation: parseInt(self.orientation()),
            signatures: sigs
        };

        $.ajax({
            url: API_BASE + '/templates/' + templateId + '/preview',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(body),
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            xhrFields: { responseType: 'blob' }
        })
        .done(function(blob) {
            var url = URL.createObjectURL(blob);
            document.getElementById('previewFrame').src = url;
            new bootstrap.Modal(document.getElementById('previewModal')).show();
            // Clean up blob URL when modal closes
            document.getElementById('previewModal').addEventListener('hidden.bs.modal', function() {
                URL.revokeObjectURL(url);
            }, { once: true });
        })
        .fail(function(xhr) {
            toastr.error('On izleme olusturulamadi: ' + (xhr.responseText || 'Hata'));
        })
        .always(function() {
            self.isPreviewing(false);
        });
    };

    // ==================== SAVE ====================

    self.saveTemplate = function() {
        if (!self.templateName()) { toastr.warning('Sablon adi gerekli'); return; }

        self.isSaving(true);

        var layout = self.fields().map(function(f) {
            return {
                FieldType: f.fieldType, DynamicKey: f.dynamicKey, StaticText: f.staticText,
                X: f.x, Y: f.y, Width: f.width, Height: f.height,
                FontFamily: f.fontFamily, FontSize: parseFloat(f.fontSize) || 14, FontColor: f.fontColor,
                IsBold: f.isBold, IsItalic: f.isItalic, TextAlign: f.textAlign
            };
        });

        var body = {
            name: self.templateName(),
            description: self.templateDesc(),
            orientation: parseInt(self.orientation()),
            layoutJson: JSON.stringify(layout),
            backgroundImageUrl: self.backgroundImageUrl
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
                var bgPromise;
                if (bgFile && savedId) {
                    var fd = new FormData();
                    fd.append('file', bgFile);
                    bgPromise = apiPost('/templates/' + savedId + '/upload-background', fd, true);
                } else {
                    bgPromise = $.Deferred().resolve();
                }

                var sigPromise;
                if (savedId) {
                    var signatures = self.templateSignatures().map(function(s) {
                        return {
                            signatureId: s.signatureId,
                            instructorName: s.instructorName(),
                            instructorTitle: s.instructorTitle(),
                            showName: s.showName(),
                            showTitle: s.showTitle(),
                            imageX: parseFloat(s.imageX()) || 0,
                            imageY: parseFloat(s.imageY()) || 0,
                            imageWidth: parseFloat(s.imageWidth()) || 12,
                            imageHeight: parseFloat(s.imageHeight()) || 8,
                            imageRotation: parseInt(s.imageRotation()) || 0,
                            nameX: parseFloat(s.nameX()) || 0,
                            nameY: parseFloat(s.nameY()) || 0,
                            titleX: parseFloat(s.titleX()) || 0,
                            titleY: parseFloat(s.titleY()) || 0,
                            nameFontSize: parseInt(s.nameFontSize()) || 8,
                            titleFontSize: parseInt(s.titleFontSize()) || 7
                        };
                    });
                    sigPromise = apiPut('/templates/' + savedId + '/signatures', { signatures: signatures });
                } else {
                    sigPromise = $.Deferred().resolve();
                }

                $.when(bgPromise, sigPromise)
                    .done(function() {
                        toastr.success('Sablon kaydedildi');
                        window.location.href = '/Panel/Templates';
                    })
                    .fail(function() {
                        toastr.warning('Sablon kaydedildi ama bazi islemler basarisiz oldu');
                        window.location.href = '/Panel/Templates';
                    });

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
