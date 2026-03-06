function DashboardViewModel() {
    var self = this;

    self.isLoading = ko.observable(true);
    self.trainingCount = ko.observable(0);
    self.templateCount = ko.observable(0);
    self.signatureCount = ko.observable(0);
    self.companyCount = ko.observable(0);

    self.loadData = function() {
        self.isLoading(true);
        $.when(
            apiGet('/trainings'),
            apiGet('/templates'),
            apiGet('/signatures'),
            apiGet('/companies')
        ).done(function(trainings, templates, signatures, companies) {
            self.trainingCount(trainings[0].length);
            self.templateCount(templates[0].length);
            self.signatureCount(signatures[0].length);
            self.companyCount(companies[0].length);
            self.isLoading(false);
        }).fail(function() {
            toastr.error('Veri yuklenemedi');
            self.isLoading(false);
        });
    };

    $(document).ready(function() {
        self.loadData();
    });
}

requireAuth() && ko.applyBindings(new DashboardViewModel(), document.getElementById('dashboardApp'));
