// templateId is set by the Razor view before this script loads
function TemplateEditorViewModel() {
    var self = this;

    // ─── State ───
    self.editorTitle = ko.observable(templateId ? 'Sablon Duzenle' : 'Yeni Sablon');
    self.templateName = ko.observable('');
    self.templateDesc = ko.observable('');
    self.orientation = ko.observable('0');
    self.backgroundPreview = ko.observable('');
    self.backgroundImageUrl = null;
    self.isSaving = ko.observable(false);
    self.isPreviewing = ko.observable(false);

    self.fields = ko.observableArray([]);
    self.templateSignatures = ko.observableArray([]);
    self.availableSignatures = ko.observableArray([]);

    self.showGuides = ko.observable(false);
    self.useSampleData = ko.observable(false);

    // ─── Font family list (kontrollu) ───
    self.availableFonts = [
        'Arial', 'Helvetica', 'Times', 'Courier',
        'Georgia', 'Verdana', 'Tahoma', 'Trebuchet MS',
        'Calibri', 'Palatino'
    ];

    // ─── Dinamik alan anahtarlari (sidebar panelinde listelenir) ───
    self.dynamicKeys = [
        { key: 'HolderName',     label: 'Katilimci Ad Soyad',  sample: 'Ahmet Yilmaz' },
        { key: 'TrainingName',   label: 'Egitim Adi',          sample: 'Is Guvenligi Egitimi' },
        { key: 'ProgramName',    label: 'Program Adi',         sample: 'Isci Saglik ve Guvenligi Programi' },
        { key: 'TrainingDate',   label: 'Tarih',               sample: '10-11.04.2026' },
        { key: 'CompanyName',    label: 'Firma Adi',           sample: 'ABC Ltd. Sti.' },
        { key: 'CertificateNo',  label: 'Sertifika No',        sample: 'CERT-20260410-0001-0001' },
        { key: 'InstructorName', label: 'Egitmen Adi',         sample: 'Dr. Mehmet Kaya' }
    ];
    // For dropdown in the selected field panel (exclude QrCode - that's its own type)
    self.dynamicKeysText = self.dynamicKeys;

    self.sampleValues = {};
    self.dynamicKeys.forEach(function(d) { self.sampleValues[d.key] = d.sample; });

    self.addableSignatures = ko.computed(function() {
        var usedIds = self.templateSignatures().map(function(ts) { return ts.signatureId; });
        return self.availableSignatures().filter(function(s) {
            return usedIds.indexOf(s.id) === -1;
        });
    });

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
    var clipboard = null;

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
        ['instructorName','instructorTitle','showName','showTitle','imageRotation','nameFontSize','titleFontSize']
            .forEach(function(k) { obs[k].subscribe(function() { self.renderAll(); }); });
        return obs;
    }

    // ─── INIT ───

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

        apiGet('/signatures')
            .done(function(data) { self.availableSignatures(data); })
            .always(function() {
                if (templateId) self.loadTemplate();
            });

        self.orientation.subscribe(function() { self.renderAll(); });
        self.useSampleData.subscribe(function() { self.renderAll(); });

        // Keyboard shortcuts
        document.addEventListener('keydown', function(e) {
            // Skip when typing in inputs
            if (/^(INPUT|TEXTAREA|SELECT)$/.test(e.target.tagName)) return;
            var sel = self.selectedItem();
            if (!sel || sel.kind !== 'field') return;

            if (e.key === 'Delete') { self.deleteSelectedField(); e.preventDefault(); }
            else if (e.ctrlKey && e.key.toLowerCase() === 'd') { self.duplicateSelectedField(); e.preventDefault(); }
            else if (e.ctrlKey && e.key.toLowerCase() === 'c') { clipboard = cloneFieldData(sel.data); toastr.info('Kopyalandi'); e.preventDefault(); }
            else if (e.ctrlKey && e.key.toLowerCase() === 'v') { if (clipboard) { pasteFromClipboard(); } e.preventDefault(); }
            else if (['ArrowLeft','ArrowRight','ArrowUp','ArrowDown'].indexOf(e.key) >= 0) {
                var step = e.shiftKey ? 2 : 0.5;
                if (e.key === 'ArrowLeft') sel.data.x = Math.max(0, sel.data.x - step);
                if (e.key === 'ArrowRight') sel.data.x = Math.min(100 - sel.data.width, sel.data.x + step);
                if (e.key === 'ArrowUp') sel.data.y = Math.max(0, sel.data.y - step);
                if (e.key === 'ArrowDown') sel.data.y = Math.min(100 - sel.data.height, sel.data.y + step);
                self.onFieldChanged();
                e.preventDefault();
            }
        });
    });

    // ─── LOAD ───

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

                var parsed = JSON.parse(tpl.layoutJson || '[]')
                    .filter(function(f) { return (f.fieldType || f.FieldType || 'static') !== 'signature'; })
                    .map(function(f, i) {
                        return self.createFieldObj(i, normalizeField(f, i));
                    });
                fieldIdCounter = parsed.length;
                self.fields(parsed);

                var sigs = (tpl.templateSignatures || []).map(makeSigObservable);
                self.templateSignatures(sigs);

                self.renderAll();
            })
            .fail(function(xhr) { toastr.error(extractError(xhr, 'Sablon yuklenemedi')); });
    };

    function normalizeField(f, idx) {
        return {
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
            isUnderline: f.isUnderline || f.IsUnderline || false,
            letterSpacing: f.letterSpacing !== undefined ? f.letterSpacing : (f.LetterSpacing !== undefined ? f.LetterSpacing : null),
            lineHeight: f.lineHeight !== undefined ? f.lineHeight : (f.LineHeight !== undefined ? f.LineHeight : null),
            textAlign: f.textAlign || f.TextAlign || 'center',
            displayOrder: f.displayOrder !== undefined ? f.displayOrder : (f.DisplayOrder !== undefined ? f.DisplayOrder : idx)
        };
    }

    self.createFieldObj = function(id, data) {
        return {
            id: id,
            fieldType: data.fieldType,
            dynamicKey: data.dynamicKey,
            staticText: data.staticText || '',
            x: parseFloat(data.x) || 0,
            y: parseFloat(data.y) || 0,
            width: parseFloat(data.width) || 20,
            height: parseFloat(data.height) || 5,
            fontFamily: data.fontFamily || 'Arial',
            fontSize: parseFloat(data.fontSize) || 14,
            fontColor: data.fontColor || '#000000',
            isBold: data.isBold || false,
            isItalic: data.isItalic || false,
            isUnderline: data.isUnderline || false,
            letterSpacing: data.letterSpacing !== undefined && data.letterSpacing !== null ? parseFloat(data.letterSpacing) : null,
            lineHeight: data.lineHeight !== undefined && data.lineHeight !== null ? parseFloat(data.lineHeight) : null,
            textAlign: data.textAlign || 'center',
            displayOrder: data.displayOrder !== undefined ? data.displayOrder : 0
        };
    };

    function cloneFieldData(f) {
        return {
            fieldType: f.fieldType, dynamicKey: f.dynamicKey, staticText: f.staticText,
            x: f.x + 3, y: f.y + 3, width: f.width, height: f.height,
            fontFamily: f.fontFamily, fontSize: f.fontSize, fontColor: f.fontColor,
            isBold: f.isBold, isItalic: f.isItalic, isUnderline: f.isUnderline,
            letterSpacing: f.letterSpacing, lineHeight: f.lineHeight,
            textAlign: f.textAlign, displayOrder: self.fields().length
        };
    }

    function pasteFromClipboard() {
        if (!clipboard) return;
        var copy = Object.assign({}, clipboard);
        copy.x = Math.min(95, copy.x + 2);
        copy.y = Math.min(95, copy.y + 2);
        copy.displayOrder = self.fields().length;
        var field = self.createFieldObj(fieldIdCounter++, copy);
        self.fields.push(field);
        self.selectItem({ kind: 'field', data: field });
    }

    // ─── ADD FIELDS ───

    self.addStaticField = function() { self.addField('static'); };
    self.addDynamicField = function() { self.addField('dynamic', 'HolderName'); };
    self.addQrCodeField = function() { self.addField('qrcode'); };
    self.addDynamicFieldWithKey = function(key) { self.addField('dynamic', key); };

    self.addField = function(type, dynamicKey) {
        var data = {
            fieldType: type,
            dynamicKey: type === 'dynamic' ? (dynamicKey || 'HolderName') : (type === 'qrcode' ? 'QrCode' : null),
            staticText: type === 'static' ? 'Metin' : '',
            x: 30, y: 30, width: 30, height: 6,
            fontFamily: 'Arial', fontSize: 16, fontColor: '#000000',
            isBold: false, isItalic: false, isUnderline: false,
            letterSpacing: null, lineHeight: null,
            textAlign: 'center', displayOrder: self.fields().length
        };
        if (type === 'qrcode') {
            data.width = 10; data.height = 10; data.x = 80; data.y = 75;
        }
        var field = self.createFieldObj(fieldIdCounter++, data);
        self.fields.push(field);
        self.selectItem({ kind: 'field', data: field });
    };

    self.duplicateSelectedField = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind !== 'field') return;
        var copy = cloneFieldData(sel.data);
        var field = self.createFieldObj(fieldIdCounter++, copy);
        self.fields.push(field);
        self.selectItem({ kind: 'field', data: field });
    };

    // ─── Layer management ───

    function reorderByDisplayOrder() {
        var arr = self.fields().slice().sort(function(a,b) { return a.displayOrder - b.displayOrder; });
        arr.forEach(function(f, i) { f.displayOrder = i; });
        self.fields(arr);
        self.renderAll();
    }

    self.bringToFront = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind !== 'field') return;
        sel.data.displayOrder = self.fields().length;
        reorderByDisplayOrder();
    };
    self.sendToBack = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind !== 'field') return;
        sel.data.displayOrder = -1;
        reorderByDisplayOrder();
    };
    self.bringForward = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind !== 'field') return;
        sel.data.displayOrder += 1.5;
        reorderByDisplayOrder();
    };
    self.sendBackward = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind !== 'field') return;
        sel.data.displayOrder -= 1.5;
        reorderByDisplayOrder();
    };

    // ─── SIGNATURE add/remove ───

    self.addSignature = function(sig) {
        var ts = makeSigObservable({
            signatureId: sig.id,
            signature: sig,
            instructorName: sig.name,
            instructorTitle: sig.title,
            showName: true, showTitle: true,
            imageX: 0, imageY: 80, imageWidth: 12, imageHeight: 8, imageRotation: 0,
            nameX: 0, nameY: 90, titleX: 0, titleY: 93
        });
        self.templateSignatures.push(ts);
        self.selectItem({ kind: 'sig-image', data: ts });
    };

    self.rotateSelectedSignature = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind.indexOf('sig-') !== 0) return;
        sel.data.imageRotation((sel.data.imageRotation() + 1) % 4);
    };

    self.removeSelectedSignature = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind.indexOf('sig-') !== 0) return;
        self.templateSignatures.remove(sel.data);
        self.selectedItem(null);
        self.renderAll();
    };

    // ─── SELECTION ───

    self.selectItem = function(item) { self.selectedItem(item); self.renderAll(); };
    self.clearSelection = function() { self.selectedItem(null); self.renderAll(); };

    self.deleteSelectedField = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind !== 'field') return;
        self.fields.remove(sel.data);
        self.selectedItem(null);
        self.renderAll();
    };

    self.onFieldChanged = function() { self.renderAll(); };

    // ─── RENDER ───

    self.renderAll = function() {
        var canvas = document.getElementById('editor-canvas');
        if (!canvas) return;
        canvas.querySelectorAll('.editor-el').forEach(function(el) { el.remove(); });

        var sel = self.selectedItem();

        // Render fields sorted by displayOrder
        self.fields().slice().sort(function(a,b) { return a.displayOrder - b.displayOrder; }).forEach(function(field) {
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
        var textContent;
        if (field.fieldType === 'dynamic') {
            if (self.useSampleData()) {
                textContent = self.sampleValues[field.dynamicKey] || '[' + field.dynamicKey + ']';
            } else {
                var keyLabels = {};
                self.dynamicKeys.forEach(function(d) { keyLabels[d.key] = d.label; });
                textContent = '[' + (keyLabels[field.dynamicKey] || field.dynamicKey) + ']';
            }
            label.style.color = self.useSampleData() ? (field.fontColor || '#000') : '#3498db';
        } else if (field.fieldType === 'qrcode') {
            textContent = '[QR]';
            label.style.color = '#27ae60';
        } else {
            textContent = field.staticText || 'Metin';
            label.style.color = field.fontColor || '#000';
        }
        label.textContent = textContent;
        label.style.fontFamily = field.fontFamily;
        label.style.fontSize = Math.min(field.fontSize * 0.8, 24) + 'px';
        label.style.fontWeight = field.isBold ? 'bold' : 'normal';
        label.style.fontStyle = field.isItalic ? 'italic' : 'normal';
        label.style.textDecoration = field.isUnderline ? 'underline' : 'none';
        label.style.textAlign = field.textAlign;
        label.style.width = '100%';
        label.style.pointerEvents = 'none';
        if (field.letterSpacing) label.style.letterSpacing = field.letterSpacing + 'px';
        if (field.lineHeight) label.style.lineHeight = field.lineHeight;
        el.appendChild(label);

        // Overflow detection
        if (self.useSampleData() && field.fieldType !== 'qrcode') {
            setTimeout(function() {
                if (label.scrollWidth > el.clientWidth + 2 || label.scrollHeight > el.clientHeight + 2) {
                    el.style.border = '2px solid #dc3545';
                    el.style.backgroundColor = 'rgba(220,53,69,0.1)';
                    el.title = 'Metin bu alana sigmiyor';
                }
            }, 0);
        }

        var handle = document.createElement('div');
        handle.style.cssText = 'position:absolute;right:0;bottom:0;width:10px;height:10px;background:#3498db;cursor:se-resize;';
        el.appendChild(handle);

        el.addEventListener('mousedown', function(e) {
            if (e.target === handle) startFieldResize(e, field);
            else startFieldDrag(e, field);
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
            if (e.target === handle) startSigResize(e, sig);
            else startSigImageDrag(e, sig);
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
        var fs = Math.min(Math.max(parseInt(sig.nameFontSize()) || 8, 6), 24);
        lbl.style.cssText = 'color:#27ae60;font-size:' + fs + 'px;pointer-events:none;white-space:nowrap;';
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
        var fs = Math.min(Math.max(parseInt(sig.titleFontSize()) || 7, 6), 24);
        lbl.style.cssText = 'color:#8e44ad;font-size:' + fs + 'px;pointer-events:none;white-space:nowrap;';
        el.appendChild(lbl);

        el.addEventListener('mousedown', function(e) {
            startSigTextDrag(e, sig, 'title');
            self.selectItem({ kind: 'sig-title', data: sig });
            e.preventDefault();
        });
        canvas.appendChild(el);
    }

    // ─── SNAP HELPERS ───

    var SNAP_TOLERANCE = 1.5; // percent

    function gatherSnapPoints(excludeField) {
        var vs = [0, 25, 50, 75, 100];  // page verticals
        var hs = [0, 25, 50, 75, 100];  // page horizontals

        self.fields().forEach(function(f) {
            if (f === excludeField) return;
            vs.push(f.x, f.x + f.width / 2, f.x + f.width);
            hs.push(f.y, f.y + f.height / 2, f.y + f.height);
        });
        self.templateSignatures().forEach(function(s) {
            vs.push(s.imageX(), s.imageX() + s.imageWidth() / 2, s.imageX() + s.imageWidth());
            hs.push(s.imageY(), s.imageY() + s.imageHeight() / 2, s.imageY() + s.imageHeight());
        });
        return { vs: vs, hs: hs };
    }

    function snapValue(val, candidates) {
        var best = null;
        candidates.forEach(function(c) {
            var d = Math.abs(val - c);
            if (d < SNAP_TOLERANCE && (best === null || d < Math.abs(val - best))) best = c;
        });
        return best;
    }

    function showSnapLine(which, percent) {
        var el = document.getElementById(which === 'v' ? 'snap-v' : 'snap-h');
        if (!el) return;
        if (percent === null) { el.style.display = 'none'; return; }
        el.style.display = 'block';
        if (which === 'v') el.style.left = percent + '%';
        else el.style.top = percent + '%';
    }

    function hideSnapLines() {
        showSnapLine('v', null);
        showSnapLine('h', null);
    }

    // ─── DRAG & DROP with snap ───

    function startFieldDrag(e, field) {
        isDragging = true;
        var canvas = document.getElementById('editor-canvas');
        var rect = canvas.getBoundingClientRect();
        dragOffsetX = (e.clientX - rect.left) / rect.width * 100 - field.x;
        dragOffsetY = (e.clientY - rect.top) / rect.height * 100 - field.y;

        var onMove = function(ev) {
            if (!isDragging) return;
            var r = canvas.getBoundingClientRect();
            var newX = (ev.clientX - r.left) / r.width * 100 - dragOffsetX;
            var newY = (ev.clientY - r.top) / r.height * 100 - dragOffsetY;

            var pts = gatherSnapPoints(field);
            var snappedX = null, snappedY = null;

            // Try snapping x edges
            var testXs = [newX, newX + field.width / 2, newX + field.width];
            for (var i = 0; i < testXs.length; i++) {
                var s = snapValue(testXs[i], pts.vs);
                if (s !== null) {
                    newX = s - (i === 0 ? 0 : (i === 1 ? field.width / 2 : field.width));
                    snappedX = s;
                    break;
                }
            }
            var testYs = [newY, newY + field.height / 2, newY + field.height];
            for (var j = 0; j < testYs.length; j++) {
                var ss = snapValue(testYs[j], pts.hs);
                if (ss !== null) {
                    newY = ss - (j === 0 ? 0 : (j === 1 ? field.height / 2 : field.height));
                    snappedY = ss;
                    break;
                }
            }

            field.x = Math.max(0, Math.min(100 - field.width, newX));
            field.y = Math.max(0, Math.min(100 - field.height, newY));
            showSnapLine('v', snappedX);
            showSnapLine('h', snappedY);
            self.renderAll();
        };
        var onUp = function() {
            isDragging = false;
            hideSnapLines();
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

    // ─── AUTO-ALIGN helpers ───

    self.centerHorizontally = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind !== 'field') return;
        sel.data.x = Math.max(0, Math.min(100 - sel.data.width, 50 - sel.data.width / 2));
        self.onFieldChanged();
    };
    self.centerVertically = function() {
        var sel = self.selectedItem();
        if (!sel || sel.kind !== 'field') return;
        sel.data.y = Math.max(0, Math.min(100 - sel.data.height, 50 - sel.data.height / 2));
        self.onFieldChanged();
    };
    self.alignAllCenterH = function() {
        // Horizontally center every field on the page
        self.fields().forEach(function(f) {
            f.x = Math.max(0, Math.min(100 - f.width, 50 - f.width / 2));
        });
        self.renderAll();
        toastr.info('Tum alanlar yatay orta eksende hizalandi');
    };
    self.distributeVertical = function() {
        var fs = self.fields().slice().sort(function(a, b) { return a.y - b.y; });
        if (fs.length < 2) return;
        var top = fs[0].y, bot = fs[fs.length - 1].y + fs[fs.length - 1].height;
        var totalH = fs.reduce(function(acc, f) { return acc + f.height; }, 0);
        var gap = (bot - top - totalH) / (fs.length - 1);
        var cursor = top;
        fs.forEach(function(f) { f.y = cursor; cursor += f.height + gap; });
        self.renderAll();
        toastr.info('Dikey bosluklar esitlendi');
    };
    self.smartArrange = function() {
        // "Sihirbazla hizala": yatay ortala + dikey bosluklari esitle
        self.alignAllCenterH();
        self.distributeVertical();
    };

    // ─── SERIALIZE ───

    function serializeLayout() {
        return self.fields().slice().sort(function(a,b){ return a.displayOrder - b.displayOrder; }).map(function(f, idx) {
            return {
                FieldType: f.fieldType, DynamicKey: f.dynamicKey, StaticText: f.staticText,
                X: f.x, Y: f.y, Width: f.width, Height: f.height,
                FontFamily: f.fontFamily, FontSize: parseFloat(f.fontSize) || 14, FontColor: f.fontColor,
                IsBold: f.isBold, IsItalic: f.isItalic, IsUnderline: f.isUnderline,
                LetterSpacing: f.letterSpacing, LineHeight: f.lineHeight,
                TextAlign: f.textAlign, DisplayOrder: idx
            };
        });
    }

    function serializeSignatures() {
        return self.templateSignatures().map(function(s) {
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
    }

    // ─── PREVIEW ───

    self.previewTemplate = function() {
        if (!templateId) { toastr.warning('Once sablonu kaydedin'); return; }
        self.isPreviewing(true);

        var body = {
            layoutJson: JSON.stringify(serializeLayout()),
            orientation: parseInt(self.orientation()),
            signatures: serializeSignatures()
        };

        $.ajax({
            url: API_BASE + '/templates/' + templateId + '/preview',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(body),
            xhrFields: { responseType: 'blob', withCredentials: true }
        })
        .done(function(blob) {
            var url = URL.createObjectURL(blob);
            document.getElementById('previewFrame').src = url;
            new bootstrap.Modal(document.getElementById('previewModal')).show();
            document.getElementById('previewModal').addEventListener('hidden.bs.modal', function() {
                URL.revokeObjectURL(url);
            }, { once: true });
        })
        .fail(function(xhr) { toastr.error(extractError(xhr, 'Onizleme olusturulamadi')); })
        .always(function() { self.isPreviewing(false); });
    };

    // ─── SAVE ───

    self.saveTemplate = function() {
        if (!self.templateName()) { toastr.warning('Sablon adi gerekli'); return; }
        self.isSaving(true);

        var body = {
            name: self.templateName(),
            description: self.templateDesc(),
            orientation: parseInt(self.orientation()),
            layoutJson: JSON.stringify(serializeLayout()),
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
                var bgPromise = bgFile && savedId
                    ? (function() {
                        var fd = new FormData();
                        fd.append('file', bgFile);
                        return apiPost('/templates/' + savedId + '/upload-background', fd, true);
                    })()
                    : $.Deferred().resolve();

                var sigPromise = savedId
                    ? apiPut('/templates/' + savedId + '/signatures', { signatures: serializeSignatures() })
                    : $.Deferred().resolve();

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
                toastr.error(extractError(xhr, 'Kaydedilemedi'));
                self.isSaving(false);
            });
    };
}

if (requireAuth()) {
    ko.applyBindings(new TemplateEditorViewModel(), document.getElementById('editorApp'));
}
