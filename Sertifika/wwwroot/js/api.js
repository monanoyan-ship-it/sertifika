const API_BASE = '/api';
let authToken = localStorage.getItem('token');

async function apiFetch(path, options = {}) {
    const headers = { ...options.headers };
    if (authToken) headers['Authorization'] = `Bearer ${authToken}`;
    if (!(options.body instanceof FormData) && options.body) {
        headers['Content-Type'] = 'application/json';
    }

    const response = await fetch(`${API_BASE}${path}`, { ...options, headers });

    if (response.status === 401) {
        logout();
        throw new Error('Oturum suresi doldu');
    }

    return response;
}

async function apiGet(path) {
    const res = await apiFetch(path);
    if (!res.ok) throw new Error(await res.text());
    return res.json();
}

async function apiPost(path, body) {
    const res = await apiFetch(path, {
        method: 'POST',
        body: body instanceof FormData ? body : JSON.stringify(body)
    });
    return res;
}

async function apiPut(path, body) {
    const res = await apiFetch(path, {
        method: 'PUT',
        body: JSON.stringify(body)
    });
    return res;
}

async function apiDelete(path) {
    const res = await apiFetch(path, { method: 'DELETE' });
    return res;
}
