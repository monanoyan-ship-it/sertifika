// Sertifika Sistemi - Shared Utilities
var API_BASE = '/api';

// Toast config
toastr.options = {
    closeButton: true,
    progressBar: true,
    positionClass: 'toast-top-right',
    timeOut: 4000
};

// ─── Browser extension console noise filter ───
// Chrome extensions (ad blockers, password managers, translators) emit
// "message channel closed" / "runtime.lastError" tracebacks that are not
// triggered by application code. Suppress them to keep the console readable.
(function() {
    var EXT_PATTERNS = [
        /message channel closed before a response was received/i,
        /runtime\.lastError/i,
        /message port closed before a response was received/i,
        /Extension context invalidated/i
    ];
    function isExtensionNoise(msg) {
        if (msg == null) return false;
        var text = (typeof msg === 'string') ? msg : (msg.message || String(msg));
        return EXT_PATTERNS.some(function(rx) { return rx.test(text); });
    }
    window.addEventListener('error', function(e) {
        if (isExtensionNoise(e.message) || isExtensionNoise(e.error)) { e.stopImmediatePropagation(); e.preventDefault(); return false; }
    }, true);
    window.addEventListener('unhandledrejection', function(e) {
        var reason = e.reason;
        if (isExtensionNoise(reason) || (reason && isExtensionNoise(reason.message))) {
            e.preventDefault();
        }
    });
    // Wrap console.error to strip extension traces (keep others intact)
    var origErr = console.error.bind(console);
    console.error = function() {
        if (arguments.length && isExtensionNoise(arguments[0])) return;
        origErr.apply(console, arguments);
    };
})();

// ─── Bootstrap modal accessibility: aria-hidden + focus fix ───
// When a modal closes while a descendant still has focus, Bootstrap logs
// "Blocked aria-hidden on an element because its descendant retained focus".
// Fix by (1) blurring any focused element inside before hide, and
// (2) using `inert` on background instead of aria-hidden on closing modal.
$(document).on('hide.bs.modal', '.modal', function() {
    var active = document.activeElement;
    if (active && this.contains(active) && typeof active.blur === 'function') {
        active.blur();
    }
});
$(document).on('shown.bs.modal', '.modal', function() {
    // Mark main app as inert so AT doesn't announce content behind the modal
    var main = document.querySelector('main');
    if (main && !main.contains(this)) main.setAttribute('inert', '');
});
$(document).on('hidden.bs.modal', '.modal', function() {
    // If no other modals remain open, remove inert
    if (!document.querySelector('.modal.show')) {
        var main = document.querySelector('main');
        if (main) main.removeAttribute('inert');
    }
});

// Current user info (set by page-level script after /auth/me)
var CURRENT_USER = null;

// Cookie helpers (for CSRF token only - auth uses HttpOnly cookie)
function getCookie(name) {
    var m = document.cookie.match(new RegExp('(?:^|; )' + name.replace(/([\.\$\?\*\|\{\}\(\)\[\]\\\/\+\^])/g, '\\$1') + '=([^;]*)'));
    return m ? decodeURIComponent(m[1]) : null;
}

// AJAX defaults - auth via HttpOnly cookie (auto-sent). CSRF via header.
$.ajaxSetup({
    xhrFields: { withCredentials: true },
    beforeSend: function(xhr, settings) {
        if (/^(POST|PUT|DELETE|PATCH)$/i.test(settings.type || '')) {
            var csrf = getCookie('XSRF-TOKEN');
            if (csrf) xhr.setRequestHeader('X-XSRF-TOKEN', csrf);
        }
    }
});

// Global 401 handler - skip if already on login page or explicitly opted out
$(document).ajaxError(function(event, xhr, settings) {
    if (xhr.status !== 401) return;
    if (settings && settings.skipAuthRedirect) return;
    if (/\/Panel\/Login/i.test(window.location.pathname)) return;
    CURRENT_USER = null;
    toastr.error('Oturum suresi doldu');
    setTimeout(function() { window.location.href = '/Panel/Login'; }, 1500);
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

function apiPut(path, data, isFormData) {
    var options = { url: API_BASE + path, method: 'PUT' };
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

function apiDelete(path) {
    return $.ajax({ url: API_BASE + path, method: 'DELETE' });
}

// jqXHR fail response'undan kullaniciya anlamli bir mesaj cikar.
// Sunucu {error: '...'} veya {message: '...'} doner; raw HTML/500 fallback.
function extractError(xhr, fallback) {
    if (!xhr) return fallback || 'Bilinmeyen hata';
    if (xhr.responseJSON) {
        return xhr.responseJSON.error || xhr.responseJSON.message || fallback || 'Islem basarisiz';
    }
    var text = (xhr.responseText || '').trim();
    if (!text) return fallback || ('HTTP ' + xhr.status);
    try {
        var parsed = JSON.parse(text);
        return parsed.error || parsed.message || fallback || 'Islem basarisiz';
    } catch (e) {}
    // HTML hata sayfasi vs. - ilk 200 karakter
    if (text.length > 200) text = text.substring(0, 200) + '...';
    return text;
}

function logout() {
    apiPost('/auth/logout').always(function() {
        CURRENT_USER = null;
        window.location.href = '/Panel/Login';
    });
}

// Page-level auth check. Loads user info; redirects to login if not authenticated.
// Pass a callback to run after auth is verified.
function requireAuth(onReady) {
    apiGet('/auth/me')
        .done(function(user) {
            CURRENT_USER = user;
            updateNavUsername();
            if (typeof onReady === 'function') onReady(user);
        })
        .fail(function(xhr) {
            if (xhr.status === 401 || xhr.status === 403) {
                window.location.href = '/Panel/Login';
            } else {
                toastr.error('Kullanici bilgisi alinamadi');
            }
        });
    // Legacy callers that use `if (requireAuth()) ko.applyBindings(...)` still work
    // because KO bindings can be applied before user info arrives.
    return true;
}

function updateNavUsername() {
    var el = document.getElementById('nav-username');
    if (el && CURRENT_USER) {
        el.innerHTML = '<i class="bi bi-person-circle me-1"></i>' +
            CURRENT_USER.firstName + ' ' + CURRENT_USER.lastName;
    }
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

function downloadAuthedFile(url, filename) {
    fetch(url, {
        method: 'GET',
        credentials: 'same-origin'
    })
    .then(function(res) {
        if (!res.ok) {
            return res.text().then(function(text) {
                var msg = text;
                try {
                    var parsed = JSON.parse(text);
                    msg = parsed.error || parsed.message || text;
                } catch (e) {}
                throw new Error(msg || ('HTTP ' + res.status));
            });
        }
        return res.blob();
    })
    .then(function(blob) {
        var blobUrl = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = blobUrl;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        setTimeout(function() { URL.revokeObjectURL(blobUrl); }, 1000);
        toastr.success('Indiriliyor: ' + filename);
    })
    .catch(function(err) {
        toastr.error('Indirilemedi: ' + (err.message || 'Bilinmeyen hata'));
    });
}

// Client-side pagination helper for KO.
// Usage in ViewModel:
//   self.pager = makePager(self.filteredItems, 20);
//   self.pagedItems = self.pager.pagedItems;
// In view: bind foreach to pagedItems, render pagination with pager.pages(), pager.currentPage, pager.setPage.
function makePager(sourceArray, pageSize) {
    var size = ko.observable(pageSize || 20);
    var current = ko.observable(1);

    var totalPages = ko.computed(function() {
        var total = (ko.unwrap(sourceArray) || []).length;
        return Math.max(1, Math.ceil(total / size()));
    });

    ko.computed(function() {
        var tp = totalPages();
        if (current() > tp) current(tp);
    });

    var pagedItems = ko.computed(function() {
        var arr = ko.unwrap(sourceArray) || [];
        var start = (current() - 1) * size();
        return arr.slice(start, start + size());
    });

    var pages = ko.computed(function() {
        var tp = totalPages();
        var cur = current();
        var result = [];
        var start = Math.max(1, cur - 2);
        var end = Math.min(tp, start + 4);
        start = Math.max(1, end - 4);
        for (var i = start; i <= end; i++) result.push(i);
        return result;
    });

    return {
        currentPage: current,
        totalPages: totalPages,
        pagedItems: pagedItems,
        pages: pages,
        setPage: function(p) {
            var tp = totalPages();
            var next = Math.min(tp, Math.max(1, p));
            current(next);
        },
        next: function() { if (current() < totalPages()) current(current() + 1); },
        prev: function() { if (current() > 1) current(current() - 1); }
    };
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('tr-TR');
}

function formatDateRange(startStr, endStr) {
    if (!startStr) return '-';
    var start = new Date(startStr);
    var pad = function(n) { return n < 10 ? '0' + n : '' + n; };
    var sd = pad(start.getDate()), sm = pad(start.getMonth() + 1), sy = start.getFullYear();
    if (!endStr) return sd + '.' + sm + '.' + sy;
    var end = new Date(endStr);
    if (end.getTime() === start.getTime()) return sd + '.' + sm + '.' + sy;
    var ed = pad(end.getDate()), em = pad(end.getMonth() + 1), ey = end.getFullYear();
    if (sy === ey && sm === em) return sd + '-' + ed + '.' + sm + '.' + sy;
    if (sy === ey) return sd + '.' + sm + '-' + ed + '.' + em + '.' + sy;
    return sd + '.' + sm + '.' + sy + '-' + ed + '.' + em + '.' + ey;
}
