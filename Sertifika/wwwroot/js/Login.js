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
            data: JSON.stringify({ email: self.email(), password: self.password() })
        })
        .done(function(data) {
            setToken(data.token);
            $.ajax({
                url: API_BASE + '/auth/me',
                method: 'GET',
                headers: { 'Authorization': 'Bearer ' + data.token }
            })
            .done(function(user) {
                localStorage.setItem('user', JSON.stringify(user));
                window.location.href = '/Panel/Dashboard';
            })
            .fail(function() {
                window.location.href = '/Panel/Dashboard';
            });
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

    if (isLoggedIn()) {
        window.location.href = '/Panel/Dashboard';
    }
}

ko.applyBindings(new LoginViewModel(), document.getElementById('loginApp'));
