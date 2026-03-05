// State
let currentUser = null;
let currentTrainingId = null;

// Toast & Confirm
function toast(message, type = 'info', duration = 4000) {
    const container = document.getElementById('toast-container');
    const el = document.createElement('div');
    el.className = `toast toast-${type}`;
    el.innerHTML = `<span>${message}</span><button class="toast-close" onclick="this.parentElement.remove()">&times;</button>`;
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
                    <button class="btn" id="confirm-no">Iptal</button>
                    <button class="btn btn-primary" id="confirm-yes">Evet</button>
                </div>
            </div>
        `;
        document.body.appendChild(overlay);
        overlay.querySelector('#confirm-yes').addEventListener('click', () => { overlay.remove(); resolve(true); });
        overlay.querySelector('#confirm-no').addEventListener('click', () => { overlay.remove(); resolve(false); });
        overlay.addEventListener('click', e => { if (e.target === overlay) { overlay.remove(); resolve(false); } });
    });
}

// Init
document.addEventListener('DOMContentLoaded', () => {
    if (authToken) {
        loadUser();
    }

    document.getElementById('login-form').addEventListener('submit', login);
    document.getElementById('logout-btn').addEventListener('click', logout);
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', e => {
            e.preventDefault();
            navigateTo(link.dataset.page);
        });
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
    document.getElementById('login-page').style.display = '';
    document.getElementById('main-app').style.display = 'none';
}

async function loadUser() {
    try {
        currentUser = await apiGet('/auth/me');
        document.getElementById('user-name').textContent = `${currentUser.firstName} ${currentUser.lastName}`;
        document.getElementById('login-page').style.display = 'none';
        document.getElementById('main-app').style.display = '';
        navigateTo('dashboard');
    } catch {
        logout();
    }
}

function showLoginError(msg) {
    const el = document.getElementById('login-error');
    el.textContent = msg;
    el.style.display = '';
}

// Navigation
function navigateTo(page) {
    document.querySelectorAll('.content-page').forEach(p => p.style.display = 'none');
    document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));

    const pageEl = document.getElementById(`page-${page}`);
    if (pageEl) pageEl.style.display = '';

    const navLink = document.querySelector(`[data-page="${page}"]`);
    if (navLink) navLink.classList.add('active');

    switch (page) {
        case 'dashboard': loadDashboard(); break;
        case 'templates': loadTemplates(); break;
        case 'signatures': loadSignatures(); break;
        case 'trainings': loadTrainings(); break;
        case 'companies': loadCompanies(); break;
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
                    <button class="btn btn-sm" onclick="editTemplate(${t.id})">Duzenle</button>
                    <button class="btn btn-danger btn-sm" onclick="deleteTemplate(${t.id})">Sil</button>
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
    document.getElementById('editor-bg').style.display = 'none';
    document.getElementById('editor-title').textContent = 'Yeni Sablon';
    updateCanvasOrientation();
    renderEditorFields();
    document.querySelectorAll('.content-page').forEach(p => p.style.display = 'none');
    document.getElementById('page-template-editor').style.display = '';
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
            bgImg.style.display = '';
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
        document.querySelectorAll('.content-page').forEach(p => p.style.display = 'none');
        document.getElementById('page-template-editor').style.display = '';
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
                ${s.imageUrl ? `<img src="${s.imageUrl}" style="max-width:150px;max-height:60px;margin-top:8px;">` : ''}
                <div class="card-actions">
                    <button class="btn btn-danger btn-sm" onclick="deleteSignature(${s.id})">Sil</button>
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
            <div class="training-card" onclick="showTrainingDetail(${t.id})" style="cursor:pointer;">
                <div class="training-info">
                    <h3>${t.name}</h3>
                    <p>${t.companyName || ''} - ${new Date(t.trainingDate).toLocaleDateString('tr-TR')}</p>
                </div>
                <div class="training-meta">
                    <span class="status-badge ${statusClasses[t.status]}">${statusLabels[t.status]}</span>
                    <p style="margin-top:5px;font-size:0.85em;color:#7f8c8d;">${t.participants?.length || 0} katilimci</p>
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
    document.querySelectorAll('.content-page').forEach(p => p.style.display = 'none');
    document.getElementById('page-training-detail').style.display = '';

    try {
        const t = await apiGet(`/trainings/${id}`);
        const statusLabels = { 0: 'Taslak', 1: 'Hazir', 2: 'Uretildi', 3: 'Dagitildi' };
        const statusClasses = { 0: 'status-draft', 1: 'status-ready', 2: 'status-generated', 3: 'status-distributed' };

        document.getElementById('training-detail-title').textContent = t.name;
        document.getElementById('training-detail-info').innerHTML = `
            <div class="card" style="margin-bottom:15px;">
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
                    <button class="btn btn-danger btn-sm" onclick="deleteParticipant(${p.id})">Sil</button>
                </td>
            </tr>
        `).join('') || '<tr><td colspan="6" style="text-align:center;">Katilimci yok</td></tr>';
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
                <p style="font-size:0.85em;color:#7f8c8d;margin-top:5px;">Sutunlar: Ad, Soyad, Email, Firma</p>
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

async function deleteParticipant(id) {
    if (!await showConfirm('Katilimciyi silmek istiyor musunuz?')) return;
    await apiDelete(`/trainings/${currentTrainingId}/participants/${id}`);
    toast('Katilimci silindi', 'success');
    loadParticipants(currentTrainingId);
}

async function generateCertificates() {
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
                    <button class="btn btn-danger btn-sm" onclick="deleteCompany(${c.id})">Sil</button>
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

// Modal
function openModal(title, bodyHtml) {
    document.getElementById('modal-title').textContent = title;
    document.getElementById('modal-body').innerHTML = bodyHtml;
    document.getElementById('modal-overlay').style.display = '';
}

function closeModal() {
    document.getElementById('modal-overlay').style.display = 'none';
}
