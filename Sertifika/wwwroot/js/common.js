// Sertifika Sistemi - Shared Utilities
var API_BASE = '/api';

// Toast config
toastr.options = {
    closeButton: true,
    progressBar: true,
    positionClass: 'toast-top-right',
    timeOut: 4000
};

// Auth
function getToken() { return localStorage.getItem('token'); }
function setToken(token) { localStorage.setItem('token', token); }
function removeToken() { localStorage.removeItem('token'); }
function isLoggedIn() { return !!getToken(); }

function requireAuth() {
    if (!isLoggedIn()) {
        window.location.href = '/Panel/Login';
        return false;
    }
    return true;
}

// AJAX - auto Bearer token
$.ajaxSetup({
    beforeSend: function(xhr) {
        var token = getToken();
        if (token) {
            xhr.setRequestHeader('Authorization', 'Bearer ' + token);
        }
    }
});

// Global 401 handler
$(document).ajaxError(function(event, xhr) {
    if (xhr.status === 401) {
        removeToken();
        localStorage.removeItem('user');
        toastr.error('Oturum suresi doldu');
        setTimeout(function() { window.location.href = '/Panel/Login'; }, 1500);
    }
});

// API Helpers
function apiGet(path) {
    return $.ajax({ url: API_BASE + path, method: 'GET' });
}

function apiPost(path, data, isFormData) {
    var options = { url: API_BASE + path, method: 'POST' };
    if (isFormData) {
        options.data = data;
        options.processData = false;
        options.contentType = false;
    } else if (data !== undefined) {
        options.data = JSON.stringify(data);
        options.contentType = 'application/json';
    }
    return $.ajax(options);
}

function apiPut(path, data) {
    return $.ajax({
        url: API_BASE + path,
        method: 'PUT',
        data: JSON.stringify(data),
        contentType: 'application/json'
    });
}

function apiDelete(path) {
    return $.ajax({ url: API_BASE + path, method: 'DELETE' });
}

function logout() {
    removeToken();
    localStorage.removeItem('user');
    window.location.href = '/Panel/Login';
}

// Confirm dialog (Bootstrap modal)
function showConfirm(message) {
    return new Promise(function(resolve) {
        var id = 'confirmModal_' + Date.now();
        var html =
            '<div class="modal fade" id="' + id + '" tabindex="-1">' +
            '<div class="modal-dialog modal-sm modal-dialog-centered">' +
            '<div class="modal-content">' +
            '<div class="modal-body"><p>' + message + '</p></div>' +
            '<div class="modal-footer">' +
            '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Iptal</button>' +
            '<button type="button" class="btn btn-primary confirm-yes">Evet</button>' +
            '</div></div></div></div>';
        document.body.insertAdjacentHTML('beforeend', html);
        var el = document.getElementById(id);
        var modal = new bootstrap.Modal(el);
        el.querySelector('.confirm-yes').addEventListener('click', function() {
            modal.hide();
            resolve(true);
        });
        el.addEventListener('hidden.bs.modal', function() {
            resolve(false);
            el.remove();
        });
        modal.show();
    });
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('tr-TR');
}
