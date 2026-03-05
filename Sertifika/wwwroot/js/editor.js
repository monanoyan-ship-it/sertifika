// Template Editor - Drag & Drop
let editorFields = [];
let currentTemplateId = null;
let selectedFieldId = null;
let isDragging = false;
let isResizing = false;
let dragOffsetX = 0;
let dragOffsetY = 0;
let fieldIdCounter = 0;

function addFieldToEditor(type) {
    const field = {
        id: fieldIdCounter++,
        fieldType: type,
        dynamicKey: type === 'dynamic' ? 'HolderName' : (type === 'qrcode' ? 'QrCode' : null),
        staticText: type === 'static' ? 'Metin' : '',
        x: 30, y: 30, width: 25, height: 5,
        fontFamily: 'Arial', fontSize: 14, fontColor: '#000000',
        isBold: false, isItalic: false, textAlign: 'center'
    };

    if (type === 'qrcode') {
        field.width = 10;
        field.height = 10;
        field.x = 80;
        field.y = 75;
    }

    editorFields.push(field);
    renderEditorFields();
    selectField(field.id);
}

function renderEditorFields() {
    const canvas = document.getElementById('editor-canvas');
    // Remove existing field elements
    canvas.querySelectorAll('.editor-field').forEach(el => el.remove());

    editorFields.forEach(field => {
        const el = document.createElement('div');
        el.className = `editor-field${field.id === selectedFieldId ? ' selected' : ''}`;
        el.dataset.fieldId = field.id;
        el.style.left = `${field.x}%`;
        el.style.top = `${field.y}%`;
        el.style.width = `${field.width}%`;
        el.style.height = `${field.height}%`;

        const label = document.createElement('span');
        label.className = 'field-label';

        if (field.fieldType === 'dynamic') {
            const keyLabels = {
                HolderName: 'Ad Soyad', TrainingName: 'Egitim Adi', TrainingDate: 'Tarih',
                CompanyName: 'Firma', CertificateNo: 'Sertifika No', QrCode: 'QR Kod'
            };
            label.textContent = `[${keyLabels[field.dynamicKey] || field.dynamicKey}]`;
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

        el.appendChild(label);

        const handle = document.createElement('div');
        handle.className = 'resize-handle';
        el.appendChild(handle);

        // Events
        el.addEventListener('mousedown', e => {
            if (e.target === handle) {
                startResize(e, field);
            } else {
                startDrag(e, field);
            }
            selectField(field.id);
            e.preventDefault();
        });

        canvas.appendChild(el);
    });
}

function selectField(id) {
    selectedFieldId = id;
    const field = editorFields.find(f => f.id === id);
    const propsEl = document.getElementById('field-props');

    if (!field) {
        propsEl.style.display = 'none';
        return;
    }

    propsEl.style.display = '';
    document.getElementById('field-type-display').textContent =
        field.fieldType === 'dynamic' ? 'Dinamik Alan' : (field.fieldType === 'qrcode' ? 'QR Kod' : 'Sabit Metin');

    document.getElementById('field-static-props').style.display = field.fieldType === 'static' ? '' : 'none';
    document.getElementById('field-dynamic-props').style.display = field.fieldType === 'dynamic' ? '' : 'none';

    if (field.fieldType === 'static') {
        document.getElementById('field-text').value = field.staticText;
    } else if (field.fieldType === 'dynamic') {
        document.getElementById('field-key').value = field.dynamicKey;
    }

    document.getElementById('field-font').value = field.fontFamily;
    document.getElementById('field-size').value = field.fontSize;
    document.getElementById('field-color').value = field.fontColor;
    document.getElementById('field-align').value = field.textAlign;
    document.getElementById('field-bold').checked = field.isBold;
    document.getElementById('field-italic').checked = field.isItalic;

    renderEditorFields();
}

function updateSelectedField() {
    const field = editorFields.find(f => f.id === selectedFieldId);
    if (!field) return;

    if (field.fieldType === 'static') {
        field.staticText = document.getElementById('field-text').value;
    } else if (field.fieldType === 'dynamic') {
        field.dynamicKey = document.getElementById('field-key').value;
    }

    field.fontFamily = document.getElementById('field-font').value;
    field.fontSize = parseFloat(document.getElementById('field-size').value);
    field.fontColor = document.getElementById('field-color').value;
    field.textAlign = document.getElementById('field-align').value;
    field.isBold = document.getElementById('field-bold').checked;
    field.isItalic = document.getElementById('field-italic').checked;

    renderEditorFields();
}

function deleteSelectedField() {
    editorFields = editorFields.filter(f => f.id !== selectedFieldId);
    selectedFieldId = null;
    document.getElementById('field-props').style.display = 'none';
    renderEditorFields();
}

function updateCanvasOrientation() {
    const canvas = document.getElementById('editor-canvas');
    const orientation = document.getElementById('tpl-orientation').value;
    canvas.className = `editor-canvas ${orientation === '0' ? 'landscape' : 'portrait'}`;
}

// Drag & Drop
function startDrag(e, field) {
    isDragging = true;
    const canvas = document.getElementById('editor-canvas');
    const rect = canvas.getBoundingClientRect();
    dragOffsetX = (e.clientX - rect.left) / rect.width * 100 - field.x;
    dragOffsetY = (e.clientY - rect.top) / rect.height * 100 - field.y;

    const onMove = ev => {
        if (!isDragging) return;
        const r = canvas.getBoundingClientRect();
        field.x = Math.max(0, Math.min(100 - field.width, (ev.clientX - r.left) / r.width * 100 - dragOffsetX));
        field.y = Math.max(0, Math.min(100 - field.height, (ev.clientY - r.top) / r.height * 100 - dragOffsetY));
        renderEditorFields();
    };

    const onUp = () => {
        isDragging = false;
        document.removeEventListener('mousemove', onMove);
        document.removeEventListener('mouseup', onUp);
    };

    document.addEventListener('mousemove', onMove);
    document.addEventListener('mouseup', onUp);
}

function startResize(e, field) {
    isResizing = true;
    const canvas = document.getElementById('editor-canvas');

    const onMove = ev => {
        if (!isResizing) return;
        const r = canvas.getBoundingClientRect();
        const newW = (ev.clientX - r.left) / r.width * 100 - field.x;
        const newH = (ev.clientY - r.top) / r.height * 100 - field.y;
        field.width = Math.max(5, Math.min(100 - field.x, newW));
        field.height = Math.max(2, Math.min(100 - field.y, newH));
        renderEditorFields();
    };

    const onUp = () => {
        isResizing = false;
        document.removeEventListener('mousemove', onMove);
        document.removeEventListener('mouseup', onUp);
    };

    document.addEventListener('mousemove', onMove);
    document.addEventListener('mouseup', onUp);
}
