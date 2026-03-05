// State
let currentUser = null;
let currentTrainingId = null;

// Toast & Confirm
function toast(message, type = 'info', duration = 4000) {
    const container = document.getElementById('toast-container');
    const el = document.createElement('div');
    el.className = `toast toast-${type}`;
    el.innerHTML = `<span>${message}</span><button class="toast-close">&times;</button>`;
    el.querySelector('.toast-close').addEventListener('click', () => el.remove());
    container.appendChild(el);
    setTimeout(() => { el.style.animation = 'toastOut 0.3s ease forwards'; setTimeout(() => el.remove(), 300); }, duration);
}

function showConfirm(message) {
    return new Promise(resolve => {
        const overlay = document.createElement('div');
        overlay.className = 'confirm-overlay';
        overlay.innerHTML = `
            <div class="confirm-box">
                <p>${message}</p>
                <div class="confirm-actions">
                    <button class="btn confirm-no">Iptal</button>
                    <button class="btn btn-primary confirm-yes">Evet</button>
                </div>
            </div>
        `;
        document.body.appendChild(overlay);
        overlay.querySelector('.confirm-yes').addEventListener('click', () => { overlay.remove(); resolve(true); });
        overlay.querySelector('.confirm-no').addEventListener('click', () => { overlay.remove(); resolve(false); });
        overlay.addEventListener('click', e => { if (e.target === overlay) { overlay.remove(); resolve(false); } });
    });
}

// Init
document.addEventListener('DOMContentLoaded', () => {
    if (authToken) {
        loadUser();
    }

    // Auth
    document.getElementById('login-form').addEventListener('submit', login);
    document.getElementById('logout-btn').addEventListener('click', logout);

    // Navigation
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', e => {
            e.preventDefault();
            navigateTo(link.dataset.page);
        });
    });

    // Page buttons
    document.getElementById('btn-new-template').addEventListener('click', showTemplateForm);
    document.getElementById('btn-save-template').addEventListener('click', saveTemplate);
    document.getElementById('btn-back-templates').addEventListener('click', () => navigateTo('templates'));
    document.getElementById('btn-new-signature').addEventListener('click', showSignatureForm);
    document.getElementById('btn-new-training').addEventListener('click', showTrainingForm);
    document.getElementById('btn-new-company').addEventListener('click', showCompanyForm);
    document.getElementById('btn-new-onedrive').addEventListener('click', showOneDriveAccountForm);
    document.getElementById('btn-preview-cert').addEventListener('click', previewCertificate);
    document.getElementById('btn-generate-certs').addEventListener('click', generateCertificates);
    document.getElementById('btn-download-zip').addEventListener('click', downloadZip);
    document.getElementById('btn-send-certs').addEventListener('click', sendCertificates);
    document.getElementById('btn-send-to-contact').addEventListener('click', showSendToContactForm);
    document.getElementById('btn-archive-onedrive').addEventListener('click', archiveToOneDrive);
    document.getElementById('btn-back-trainings').addEventListener('click', () => navigateTo('trainings'));
    document.getElementById('btn-add-participant').addEventListener('click', showParticipantForm);
    document.getElementById('btn-upload-excel').addEventListener('click', showExcelUpload);

    // Modal
    document.getElementById('modal-overlay').addEventListener('click', e => {
        if (e.target === document.getElementById('modal-overlay')) closeModal();
    });
    document.getElementById('modal-content').addEventListener('click', e => e.stopPropagation());
    document.getElementById('btn-modal-close').addEventListener('click', closeModal);

    // Dynamic content event delegation
    document.getElementById('app').addEventListener('click', e => {
        const target = e.target.closest('[data-action]');
        if (!target) return;
        const id = parseInt(target.dataset.id);
        switch (target.dataset.action) {
            case 'edit-template': editTemplate(id); break;
            case 'delete-template': deleteTemplate(id); break;
            case 'delete-signature': deleteSignature(id); break;
            case 'show-training': showTrainingDetail(id); break;
            case 'edit-participant': editParticipant(id); break;
            case 'delete-participant': deleteParticipant(id); break;
            case 'delete-company': deleteCompany(id); break;
            case 'set-default-onedrive': setDefaultOneDrive(id); break;
            case 'delete-onedrive': deleteOneDriveAccount(id); break;
        }
    });
});

// Auth
async function login(e) {
    e.preventDefault();
    const email = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;

    try {
        const res = await fetch(`${API_BASE}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        if (!res.ok) {
            const err = await res.json();
            showLoginError(err.message || 'Giris basarisiz');
            return;
        }

        const data = await res.json();
        authToken = data.token;
        localStorage.setItem('token', authToken);
        loadUser();
    } catch (err) {
        showLoginError('Baglanti hatasi');
    }
}

function logout() {
    authToken = null;
    currentUser = null;
    localStorage.removeItem('token');
    document.getElementById('login-page').classList.remove('hidden');
    document.getElementById('main-app').classList.add('hidden');
}

async function loadUser() {
    try {
        currentUser = await apiGet('/auth/me');
        document.getElementById('user-name').textContent = `${currentUser.firstName} ${currentUser.lastName}`;
        document.getElementById('login-page').classList.add('hidden');
        document.getElementById('main-app').classList.remove('hidden');
        navigateTo('dashboard');
    } catch {
        logout();
    }
}

function showLoginError(msg) {
    const el = document.getElementById('login-error');
    el.textContent = msg;
    el.classList.remove('hidden');
}

// Navigation
function navigateTo(page) {
    document.querySelectorAll('.content-page').forEach(p => p.classList.add('hidden'));
    document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));

    const pageEl = document.getElementById(`page-${page}`);
    if (pageEl) pageEl.classList.remove('hidden');

    const navLink = document.querySelector(`[data-page="${page}"]`);
    if (navLink) navLink.classList.add('active');

    switch (page) {
        case 'dashboard': loadDashboard(); break;
        case 'templates': loadTemplates(); break;
        case 'signatures': loadSignatures(); break;
        case 'trainings': loadTrainings(); break;
        case 'companies': loadCompanies(); break;
        case 'settings': loadSettings(); break;
    }
}

// Dashboard
async function loadDashboard() {
    const grid = document.getElementById('stats-grid');
    grid.innerHTML = '<div class="loading">Yukleniyor...</div>';

    try {
        const [trainings, templates, signatures, companies] = await Promise.all([
            apiGet('/trainings'),
            apiGet('/templates'),
            apiGet('/signatures'),
            apiGet('/companies')
        ]);

        grid.innerHTML = `
            <div class="stat-card"><h3>Egitimler</h3><div class="stat-value">${trainings.length}</div></div>
            <div class="stat-card"><h3>Sablonlar</h3><div class="stat-value">${templates.length}</div></div>
            <div class="stat-card"><h3>Imzalar</h3><div class="stat-value">${signatures.length}</div></div>
            <div class="stat-card"><h3>Firmalar</h3><div class="stat-value">${companies.length}</div></div>
        `;
    } catch (err) {
        grid.innerHTML = `<div class="error-msg">Veri yuklenemedi: ${err.message}</div>`;
    }
}

// Templates
async function loadTemplates() {
    const list = document.getElementById('templates-list');
    list.innerHTML = '<div class="loading">Yukleniyor...</div>';

    try {
        const templates = await apiGet('/templates');
        list.innerHTML = templates.map(t => `
            <div class="card">
                <h3>${t.name}</h3>
                <p>${t.description || ''}</p>
                <p>Yonelim: ${t.orientation === 0 ? 'Yatay' : 'Dikey'}</p>
                <div class="card-actions">
                    <button class="btn btn-sm" data-action="edit-template" data-id="${t.id}">Duzenle</button>
                    <button class="btn btn-danger btn-sm" data-action="delete-template" data-id="${t.id}">Sil</button>
                </div>
            </div>
        `).join('') || '<p>Henuz sablon yok.</p>';
    } catch (err) {
        list.innerHTML = `<div class="error-msg">${err.message}</div>`;
    }
}

function showTemplateForm() {
    currentTemplateId = null;
    editorFields = [];
    document.getElementById('tpl-name').value = '';
    document.getElementById('tpl-desc').value = '';
    document.getElementById('tpl-orientation').value = '0';
    document.getElementById('editor-bg').classList.add('hidden');
    document.getElementById('editor-title').textContent = 'Yeni Sablon';
    updateCanvasOrientation();
    renderEditorFields();
    document.querySelectorAll('.content-page').forEach(p => p.classList.add('hidden'));
    document.getElementById('page-template-editor').classList.remove('hidden');
}

async function editTemplate(id) {
    try {
        const tpl = await apiGet(`/templates/${id}`);
        currentTemplateId = id;
        document.getElementById('tpl-name').value = tpl.name;
        document.getElementById('tpl-desc').value = tpl.description || '';
        document.getElementById('tpl-orientation').value = tpl.orientation;

        if (tpl.backgroundImageUrl) {
            const bgImg = document.getElementById('editor-bg');
            bgImg.src = tpl.backgroundImageUrl;
            bgImg.classList.remove('hidden');
        }

        editorFields = JSON.parse(tpl.layoutJson || '[]').map((f, i) => ({
            id: i,
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
        }));

        document.getElementById('editor-title').textContent = `Sablon: ${tpl.name}`;
        updateCanvasOrientation();
        renderEditorFields();
        document.querySelectorAll('.content-page').forEach(p => p.classList.add('hidden'));
        document.getElementById('page-template-editor').classList.remove('hidden');
    } catch (err) {
        toast('Sablon yuklenemedi: ' + err.message, 'error');
    }
}

async function saveTemplate() {
    const name = document.getElementById('tpl-name').value;
    if (!name) { toast('Sablon adi gerekli', 'warning'); return; }

    const layout = editorFields.map(f => ({
        FieldType: f.fieldType,
        DynamicKey: f.dynamicKey,
        StaticText: f.staticText,
        X: f.x, Y: f.y, Width: f.width, Height: f.height,
        FontFamily: f.fontFamily, FontSize: f.fontSize, FontColor: f.fontColor,
        IsBold: f.isBold, IsItalic: f.isItalic, TextAlign: f.textAlign
    }));

    const body = {
        name,
        description: document.getElementById('tpl-desc').value,
        orientation: parseInt(document.getElementById('tpl-orientation').value),
        layoutJson: JSON.stringify(layout)
    };

    try {
        if (currentTemplateId) {
            body.id = currentTemplateId;
            await apiPut(`/templates/${currentTemplateId}`, body);
        } else {
            const res = await apiPost('/templates', body);
            if (!res.ok) throw new Error(await res.text());
            const created = await res.json();
            currentTemplateId = created.id;
        }

        const bgFile = document.getElementById('tpl-bg-file').files[0];
        if (bgFile && currentTemplateId) {
            const formData = new FormData();
            formData.append('file', bgFile);
            await apiPost(`/templates/${currentTemplateId}/upload-background`, formData);
        }

        toast('Sablon kaydedildi', 'success');
        navigateTo('templates');
    } catch (err) {
        toast('Hata: ' + err.message, 'error');
    }
}

async function deleteTemplate(id) {
    if (!await showConfirm('Sablonu silmek istiyor musunuz?')) return;
    await apiDelete(`/templates/${id}`);
    toast('Sablon silindi', 'success');
    loadTemplates();
}

// Signatures
async function loadSignatures() {
    const list = document.getElementById('signatures-list');
    list.innerHTML = '<div class="loading">Yukleniyor...</div>';

    try {
        const sigs = await apiGet('/signatures');
        list.innerHTML = sigs.map(s => `
            <div class="card">
                <h3>${s.name}</h3>
                <p>${s.title}</p>
                ${s.imageUrl ? `<img src="${s.imageUrl}" class="sig-preview" alt="">` : ''}
                <div class="card-actions">
                    <button class="btn btn-danger btn-sm" data-action="delete-signature" data-id="${s.id}">Sil</button>
                </div>
            </div>
        `).join('') || '<p>Henuz imza yok.</p>';
    } catch (err) {
        list.innerHTML = `<div class="error-msg">${err.message}</div>`;
    }
}

function showSignatureForm() {
    openModal('Yeni Imza', `
        <form id="signature-form">
            <div class="form-group"><label>Ad Soyad</label><input type="text" id="sig-name" required></div>
            <div class="form-group"><label>Unvan</label><input type="text" id="sig-title"></div>
            <div class="form-group"><label>Imza Gorseli</label><input type="file" id="sig-file" accept="image/*" required></div>
            <button type="submit" class="btn btn-primary">Kaydet</button>
        </form>
    `);
    document.getElementById('signature-form').addEventListener('submit', async e => {
        e.preventDefault();
        const formData = new FormData();
        formData.append('Name', document.getElementById('sig-name').value);
        formData.append('Title', document.getElementById('sig-title').value);
        formData.append('file', document.getElementById('sig-file').files[0]);

        const res = await apiPost('/signatures', formData);
        if (res.ok) { closeModal(); toast('Imza eklendi', 'success'); loadSignatures(); }
        else toast('Hata: ' + await res.text(), 'error');
    });
}

async function deleteSignature(id) {
    if (!await showConfirm('Imzayi silmek istiyor musunuz?')) return;
    await apiDelete(`/signatures/${id}`);
    toast('Imza silindi', 'success');
    loadSignatures();
}

// Trainings
async function loadTrainings() {
    const list = document.getElementById('trainings-list');
    list.innerHTML = '<div class="loading">Yukleniyor...</div>';

    try {
        const trainings = await apiGet('/trainings');
        const statusLabels = { 0: 'Taslak', 1: 'Hazir', 2: 'Uretildi', 3: 'Dagitildi' };
        const statusClasses = { 0: 'status-draft', 1: 'status-ready', 2: 'status-generated', 3: 'status-distributed' };

        list.innerHTML = trainings.map(t => `
            <div class="training-card clickable" data-action="show-training" data-id="${t.id}">
                <div class="training-info">
                    <h3>${t.name}</h3>
                    <p>${t.companyName || ''} - ${new Date(t.trainingDate).toLocaleDateString('tr-TR')}</p>
                </div>
                <div class="training-meta">
                    <span class="status-badge ${statusClasses[t.status]}">${statusLabels[t.status]}</span>
                    <p class="meta-text">${t.participants?.length || 0} katilimci</p>
                </div>
            </div>
        `).join('') || '<p>Henuz egitim yok.</p>';
    } catch (err) {
        list.innerHTML = `<div class="error-msg">${err.message}</div>`;
    }
}

function showTrainingForm() {
    Promise.all([apiGet('/templates'), apiGet('/signatures')]).then(([templates, signatures]) => {
        const tplOptions = templates.map(t => `<option value="${t.id}">${t.name}</option>`).join('');
        const sigCheckboxes = signatures.map(s =>
            `<label><input type="checkbox" value="${s.id}" class="sig-check"> ${s.name} (${s.title})</label><br>`
        ).join('');

        openModal('Yeni Egitim', `
            <form id="training-form">
                <div class="form-group"><label>Egitim Adi</label><input type="text" id="tr-name" required></div>
                <div class="form-group"><label>Aciklama</label><textarea id="tr-desc"></textarea></div>
                <div class="form-group"><label>Tarih</label><input type="date" id="tr-date" required></div>
                <div class="form-group"><label>Firma Adi</label><input type="text" id="tr-company"></div>
                <div class="form-group"><label>Sablon</label><select id="tr-template">${tplOptions}</select></div>
                <div class="form-group"><label>Imzalar</label><div>${sigCheckboxes || 'Imza bulunamadi'}</div></div>
                <button type="submit" class="btn btn-primary">Olustur</button>
            </form>
        `);
        document.getElementById('training-form').addEventListener('submit', async e => {
            e.preventDefault();
            const sigIds = [...document.querySelectorAll('.sig-check:checked')].map(c => parseInt(c.value));
            const body = {
                name: document.getElementById('tr-name').value,
                description: document.getElementById('tr-desc').value,
                trainingDate: document.getElementById('tr-date').value + 'T00:00:00Z',
                companyName: document.getElementById('tr-company').value,
                templateId: parseInt(document.getElementById('tr-template').value),
                signatureIds: sigIds
            };
            const res = await apiPost('/trainings', body);
            if (res.ok) { closeModal(); toast('Egitim olusturuldu', 'success'); loadTrainings(); }
            else toast('Hata: ' + await res.text(), 'error');
        });
    });
}

// Training Detail
async function showTrainingDetail(id) {
    currentTrainingId = id;
    document.querySelectorAll('.content-page').forEach(p => p.classList.add('hidden'));
    document.getElementById('page-training-detail').classList.remove('hidden');

    try {
        const t = await apiGet(`/trainings/${id}`);
        const statusLabels = { 0: 'Taslak', 1: 'Hazir', 2: 'Uretildi', 3: 'Dagitildi' };
        const statusClasses = { 0: 'status-draft', 1: 'status-ready', 2: 'status-generated', 3: 'status-distributed' };

        document.getElementById('training-detail-title').textContent = t.name;
        document.getElementById('training-detail-info').innerHTML = `
            <div class="card detail-card">
                <p><strong>Tarih:</strong> ${new Date(t.trainingDate).toLocaleDateString('tr-TR')}</p>
                <p><strong>Firma:</strong> ${t.companyName || '-'}</p>
                <p><strong>Sablon:</strong> ${t.template?.name || '-'}</p>
                <p><strong>Durum:</strong> <span class="status-badge ${statusClasses[t.status]}">${statusLabels[t.status]}</span></p>
                <p><strong>Imzalar:</strong> ${t.trainingSignatures?.map(ts => ts.signature?.name).join(', ') || '-'}</p>
            </div>
        `;

        loadParticipants(id);
    } catch (err) {
        toast('Egitim yuklenemedi: ' + err.message, 'error');
    }
}

async function loadParticipants(trainingId) {
    try {
        const participants = await apiGet(`/trainings/${trainingId}/participants`);
        const tbody = document.querySelector('#participants-table tbody');
        tbody.innerHTML = participants.map(p => `
            <tr>
                <td>${p.firstName}</td>
                <td>${p.lastName}</td>
                <td>${p.email || '-'}</td>
                <td>${p.companyName || '-'}</td>
                <td>${p.certificateNumber || '-'}</td>
                <td>
                    ${p.certificatePdfUrl ? `<a href="${p.certificatePdfUrl}" target="_blank" class="btn btn-sm">PDF</a>` : ''}
                    <button class="btn btn-sm" data-action="edit-participant" data-id="${p.id}">Duzenle</button>
                    <button class="btn btn-danger btn-sm" data-action="delete-participant" data-id="${p.id}">Sil</button>
                </td>
            </tr>
        `).join('') || '<tr><td colspan="6" class="text-center">Katilimci yok</td></tr>';
    } catch (err) {
        console.error(err);
    }
}

function showParticipantForm() {
    openModal('Katilimci Ekle', `
        <form id="participant-form">
            <div class="form-group"><label>Ad</label><input type="text" id="p-fname" required></div>
            <div class="form-group"><label>Soyad</label><input type="text" id="p-lname" required></div>
            <div class="form-group"><label>E-posta</label><input type="email" id="p-email"></div>
            <div class="form-group"><label>Firma</label><input type="text" id="p-company"></div>
            <button type="submit" class="btn btn-primary">Ekle</button>
        </form>
    `);
    document.getElementById('participant-form').addEventListener('submit', async e => {
        e.preventDefault();
        const body = {
            firstName: document.getElementById('p-fname').value,
            lastName: document.getElementById('p-lname').value,
            email: document.getElementById('p-email').value,
            companyName: document.getElementById('p-company').value,
            trainingId: currentTrainingId
        };
        const res = await apiPost(`/trainings/${currentTrainingId}/participants`, body);
        if (res.ok) { closeModal(); toast('Katilimci eklendi', 'success'); loadParticipants(currentTrainingId); }
        else toast('Hata: ' + await res.text(), 'error');
    });
}

function showExcelUpload() {
    openModal('Excel Yukle', `
        <form id="excel-form">
            <div class="form-group">
                <label>Excel Dosyasi (.xlsx)</label>
                <input type="file" id="excel-file" accept=".xlsx,.xls" required>
                <p class="hint-text">Sutunlar: Ad, Soyad, Email, Firma</p>
            </div>
            <button type="submit" class="btn btn-primary">Yukle</button>
        </form>
    `);
    document.getElementById('excel-form').addEventListener('submit', async e => {
        e.preventDefault();
        const formData = new FormData();
        formData.append('file', document.getElementById('excel-file').files[0]);
        const res = await apiPost(`/trainings/${currentTrainingId}/participants/import-excel`, formData);
        if (res.ok) { closeModal(); toast('Katilimcilar yuklendi', 'success'); loadParticipants(currentTrainingId); }
        else toast('Hata: ' + await res.text(), 'error');
    });
}

async function editParticipant(id) {
    try {
        const p = await apiGet(`/trainings/${currentTrainingId}/participants/${id}`);
        openModal('Katilimci Duzenle', `
            <form id="edit-participant-form">
                <div class="form-group"><label>Ad</label><input type="text" id="ep-fname" value="${p.firstName}" required></div>
                <div class="form-group"><label>Soyad</label><input type="text" id="ep-lname" value="${p.lastName}" required></div>
                <div class="form-group"><label>E-posta</label><input type="email" id="ep-email" value="${p.email || ''}"></div>
                <div class="form-group"><label>Firma</label><input type="text" id="ep-company" value="${p.companyName || ''}"></div>
                <button type="submit" class="btn btn-primary">Kaydet</button>
            </form>
        `);
        document.getElementById('edit-participant-form').addEventListener('submit', async e => {
            e.preventDefault();
            const body = {
                id: p.id,
                firstName: document.getElementById('ep-fname').value,
                lastName: document.getElementById('ep-lname').value,
                email: document.getElementById('ep-email').value,
                companyName: document.getElementById('ep-company').value,
                trainingId: currentTrainingId
            };
            const res = await apiPut(`/trainings/${currentTrainingId}/participants/${id}`, body);
            if (res.ok) { closeModal(); toast('Katilimci guncellendi', 'success'); loadParticipants(currentTrainingId); }
            else toast('Hata: ' + await res.text(), 'error');
        });
    } catch (err) {
        toast('Katilimci yuklenemedi: ' + err.message, 'error');
    }
}

async function deleteParticipant(id) {
    if (!await showConfirm('Katilimciyi silmek istiyor musunuz?')) return;
    await apiDelete(`/trainings/${currentTrainingId}/participants/${id}`);
    toast('Katilimci silindi', 'success');
    loadParticipants(currentTrainingId);
}

async function generateCertificates() {
    // Long name check
    try {
        const participants = await apiGet(`/trainings/${currentTrainingId}/participants`);
        const longNames = participants.filter(p => `${p.firstName} ${p.lastName}`.length > 40);
        if (longNames.length > 0) {
            toast(`Uyari: ${longNames.length} katilimcinin ad soyadi 40 karakterden uzun. Metin tasmasi olabilir.`, 'warning', 6000);
        }
    } catch { /* continue anyway */ }

    if (!await showConfirm('Sertifikalar uretilecek. Devam etmek istiyor musunuz?')) return;
    try {
        const res = await apiPost(`/trainings/${currentTrainingId}/generate`);
        if (!res.ok) throw new Error(await res.text());
        const result = await res.json();
        toast(`Uretim tamamlandi: ${result.success}/${result.total} basarili`, 'success', 6000);
        showTrainingDetail(currentTrainingId);
    } catch (err) {
        toast('Hata: ' + err.message, 'error');
    }
}

async function previewCertificate() {
    window.open(`${API_BASE}/trainings/${currentTrainingId}/preview?access_token=${authToken}`, '_blank');
}

async function downloadZip() {
    try {
        const res = await apiFetch(`/trainings/${currentTrainingId}/download-zip`);
        if (!res.ok) throw new Error(await res.text());
        const blob = await res.blob();
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `certificates_training_${currentTrainingId}.zip`;
        a.click();
        URL.revokeObjectURL(url);
        toast('ZIP dosyasi indiriliyor', 'success');
    } catch (err) {
        toast('Hata: ' + err.message, 'error');
    }
}

async function sendCertificates() {
    if (!await showConfirm('Sertifikalar katilimcilara e-posta ile gonderilecek. Devam?')) return;
    try {
        const res = await apiPost(`/trainings/${currentTrainingId}/send-certificates`);
        if (!res.ok) throw new Error(await res.text());
        const result = await res.json();
        toast(`Gonderim: ${result.sent}/${result.total} basarili`, 'success', 6000);
        showTrainingDetail(currentTrainingId);
    } catch (err) {
        toast('Hata: ' + err.message, 'error');
    }
}

// Send to Contact
function showSendToContactForm() {
    openModal('Kisiye Gonder', `
        <form id="contact-form">
            <div class="form-group"><label>Alici Adi</label><input type="text" id="ct-name" required></div>
            <div class="form-group"><label>E-posta</label><input type="email" id="ct-email" required></div>
            <p class="hint-text">Tum sertifikalar bu kisiye gonderilecektir.</p>
            <button type="submit" class="btn btn-primary">Gonder</button>
        </form>
    `);
    document.getElementById('contact-form').addEventListener('submit', async e => {
        e.preventDefault();
        const body = {
            name: document.getElementById('ct-name').value,
            email: document.getElementById('ct-email').value
        };
        try {
            const res = await apiPost(`/trainings/${currentTrainingId}/send-to-contact`, body);
            if (!res.ok) throw new Error(await res.text());
            closeModal();
            toast('Sertifikalar gonderildi', 'success');
        } catch (err) {
            toast('Hata: ' + err.message, 'error');
        }
    });
}

// OneDrive Archive
async function archiveToOneDrive() {
    if (!await showConfirm('Sertifikalar OneDrive\'a arsivlenecek. Devam?')) return;
    try {
        const res = await apiPost(`/trainings/${currentTrainingId}/archive-onedrive`);
        if (!res.ok) throw new Error(await res.text());
        const result = await res.json();
        toast(`Arsivleme: ${result.uploaded}/${result.total} yuklendi. Klasor: ${result.folderPath}`, 'success', 6000);
    } catch (err) {
        toast('Hata: ' + err.message, 'error');
    }
}

// Companies
async function loadCompanies() {
    const list = document.getElementById('companies-list');
    list.innerHTML = '<div class="loading">Yukleniyor...</div>';

    try {
        const companies = await apiGet('/companies');
        list.innerHTML = companies.map(c => `
            <div class="card">
                <h3>${c.name}</h3>
                <p>${c.contactEmail || ''}</p>
                <p>${c.contactPhone || ''}</p>
                <p>${c.address || ''}</p>
                <div class="card-actions">
                    <button class="btn btn-danger btn-sm" data-action="delete-company" data-id="${c.id}">Sil</button>
                </div>
            </div>
        `).join('') || '<p>Henuz firma yok.</p>';
    } catch (err) {
        list.innerHTML = `<div class="error-msg">${err.message}</div>`;
    }
}

function showCompanyForm() {
    openModal('Yeni Firma', `
        <form id="company-form">
            <div class="form-group"><label>Firma Adi</label><input type="text" id="c-name" required></div>
            <div class="form-group"><label>E-posta</label><input type="email" id="c-email"></div>
            <div class="form-group"><label>Telefon</label><input type="text" id="c-phone"></div>
            <div class="form-group"><label>Adres</label><textarea id="c-address"></textarea></div>
            <button type="submit" class="btn btn-primary">Kaydet</button>
        </form>
    `);
    document.getElementById('company-form').addEventListener('submit', async e => {
        e.preventDefault();
        const body = {
            name: document.getElementById('c-name').value,
            contactEmail: document.getElementById('c-email').value,
            contactPhone: document.getElementById('c-phone').value,
            address: document.getElementById('c-address').value
        };
        const res = await apiPost('/companies', body);
        if (res.ok) { closeModal(); toast('Firma eklendi', 'success'); loadCompanies(); }
        else toast('Hata: ' + await res.text(), 'error');
    });
}

async function deleteCompany(id) {
    if (!await showConfirm('Firmayi silmek istiyor musunuz?')) return;
    await apiDelete(`/companies/${id}`);
    toast('Firma silindi', 'success');
    loadCompanies();
}

// Settings
async function loadSettings() {
    await loadOneDriveAccounts();
}

async function loadOneDriveAccounts() {
    const list = document.getElementById('onedrive-accounts-list');
    list.innerHTML = '<div class="loading">Yukleniyor...</div>';

    try {
        const accounts = await apiGet('/onedrive-accounts');
        list.innerHTML = accounts.map(a => `
            <div class="card">
                <h3>${a.name} ${a.isDefault ? '<span class="status-badge status-ready">Varsayilan</span>' : ''}</h3>
                <p>Tenant: ${a.tenantId}</p>
                <p>Client: ${a.clientId}</p>
                <p>Drive User: ${a.driveUserId}</p>
                <div class="card-actions">
                    ${!a.isDefault ? `<button class="btn btn-success btn-sm" data-action="set-default-onedrive" data-id="${a.id}">Varsayilan Yap</button>` : ''}
                    <button class="btn btn-danger btn-sm" data-action="delete-onedrive" data-id="${a.id}">Sil</button>
                </div>
            </div>
        `).join('') || '<p>Henuz OneDrive hesabi yok.</p>';
    } catch (err) {
        list.innerHTML = `<div class="error-msg">${err.message}</div>`;
    }
}

function showOneDriveAccountForm() {
    openModal('Yeni OneDrive Hesabi', `
        <form id="onedrive-form">
            <div class="form-group"><label>Hesap Adi</label><input type="text" id="od-name" required></div>
            <div class="form-group"><label>Tenant ID</label><input type="text" id="od-tenant" required></div>
            <div class="form-group"><label>Client ID</label><input type="text" id="od-client" required></div>
            <div class="form-group"><label>Client Secret</label><input type="password" id="od-secret" required></div>
            <div class="form-group"><label>Drive User ID</label><input type="text" id="od-user" required></div>
            <div class="form-group"><label><input type="checkbox" id="od-default"> Varsayilan hesap</label></div>
            <button type="submit" class="btn btn-primary">Kaydet</button>
        </form>
    `);
    document.getElementById('onedrive-form').addEventListener('submit', async e => {
        e.preventDefault();
        const body = {
            name: document.getElementById('od-name').value,
            tenantId: document.getElementById('od-tenant').value,
            clientId: document.getElementById('od-client').value,
            clientSecret: document.getElementById('od-secret').value,
            driveUserId: document.getElementById('od-user').value,
            isDefault: document.getElementById('od-default').checked
        };
        const res = await apiPost('/onedrive-accounts', body);
        if (res.ok) { closeModal(); toast('OneDrive hesabi eklendi', 'success'); loadOneDriveAccounts(); }
        else toast('Hata: ' + await res.text(), 'error');
    });
}

async function setDefaultOneDrive(id) {
    const res = await apiPost(`/onedrive-accounts/${id}/set-default`);
    if (res.ok) { toast('Varsayilan hesap ayarlandi', 'success'); loadOneDriveAccounts(); }
    else toast('Hata: ' + await res.text(), 'error');
}

async function deleteOneDriveAccount(id) {
    if (!await showConfirm('OneDrive hesabini silmek istiyor musunuz?')) return;
    await apiDelete(`/onedrive-accounts/${id}`);
    toast('Hesap silindi', 'success');
    loadOneDriveAccounts();
}

// Modal
function openModal(title, bodyHtml) {
    document.getElementById('modal-title').textContent = title;
    document.getElementById('modal-body').innerHTML = bodyHtml;
    document.getElementById('modal-overlay').classList.remove('hidden');
}

function closeModal() {
    document.getElementById('modal-overlay').classList.add('hidden');
}
