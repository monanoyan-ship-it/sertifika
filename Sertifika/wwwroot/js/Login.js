function LoginViewModel() {
    var self = this;

    self.email = ko.observable('');
    self.password = ko.observable('');
    self.errorMessage = ko.observable('');
    self.isLoading = ko.observable(false);

    self.login = function() {
        self.errorMessage('');
        self.isLoading(true);

        $.ajax({
            url: API_BASE + '/auth/login',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ email: self.email(), password: self.password() }),
            xhrFields: { withCredentials: true },
            skipAuthRedirect: true
        })
            .done(function() {
                window.location.href = '/Panel/Dashboard';
            })
            .fail(function(xhr) {
                var msg = 'Giris basarisiz';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    msg = xhr.responseJSON.message;
                }
                self.errorMessage(msg);
                self.isLoading(false);
            });
    };

    // Silently check if already authenticated; skip global 401 handler.
    $.ajax({
        url: API_BASE + '/auth/me',
        method: 'GET',
        xhrFields: { withCredentials: true },
        skipAuthRedirect: true
    }).done(function() {
        window.location.href = '/Panel/Dashboard';
    });
}

ko.applyBindings(new LoginViewModel(), document.getElementById('loginApp'));
