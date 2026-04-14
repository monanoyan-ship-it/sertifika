function LoginViewModel() {
    var self = this;

    self.email = ko.observable('');
    self.password = ko.observable('');
    self.errorMessage = ko.observable('');
    self.isLoading = ko.observable(false);

    self.login = function() {
        self.errorMessage('');
        self.isLoading(true);

        apiPost('/auth/login', { email: self.email(), password: self.password() })
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

    // If already authenticated, skip login
    apiGet('/auth/me')
        .done(function() { window.location.href = '/Panel/Dashboard'; });
}

ko.applyBindings(new LoginViewModel(), document.getElementById('loginApp'));
