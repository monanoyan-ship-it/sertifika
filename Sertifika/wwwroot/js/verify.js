// Check URL params for auto-verify
const params = new URLSearchParams(window.location.search);
const certNo = params.get('no');
if (certNo) {
    document.getElementById('cert-number').value = certNo;
    verifyCert(certNo);
}

document.getElementById('verify-form').addEventListener('submit', e => {
    e.preventDefault();
    verifyCert(document.getElementById('cert-number').value);
});

async function verifyCert(number) {
    const result = document.getElementById('verify-result');
    result.innerHTML = '<div class="loading">Dogrulaniyor...</div>';

    try {
        const res = await fetch(`/api/certificates/verify/${encodeURIComponent(number)}`);
        const data = await res.json();

        if (data.valid) {
            result.innerHTML = `
                <div class="verify-valid">
                    <h2 class="verify-heading">Sertifika Gecerli</h2>
                    <table class="verify-table">
                        <tr><td class="verify-label">Sertifika No:</td><td>${data.certificate.certificateNumber}</td></tr>
                        <tr><td class="verify-label">Katilimci:</td><td>${data.certificate.holderName}</td></tr>
                        <tr><td class="verify-label">Egitim:</td><td>${data.certificate.trainingName}</td></tr>
                        <tr><td class="verify-label">Tarih:</td><td>${data.certificate.trainingDate}</td></tr>
                        <tr><td class="verify-label">Firma:</td><td>${data.certificate.companyName || '-'}</td></tr>
                    </table>
                </div>
            `;
        } else {
            result.innerHTML = `<div class="verify-invalid"><h2>Sertifika Bulunamadi</h2><p>${data.message}</p></div>`;
        }
    } catch {
        result.innerHTML = '<div class="verify-invalid"><h2>Baglanti hatasi</h2></div>';
    }
}
