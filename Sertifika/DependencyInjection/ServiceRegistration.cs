using Sertifika.EntityServices;
using Sertifika.Factories.Categories;
using Sertifika.Factories.Certificates;
using Sertifika.Factories.Auth;
using Sertifika.Factories.Holders;
using Sertifika.Factories.Participants;
using Sertifika.Factories.Signatures;
using Sertifika.Factories.Templates;
using Sertifika.Factories.Trainings;
using Sertifika.Infrastructure;

namespace Sertifika.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Infrastructure
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Entity Services
        services.AddScoped<ICertificateEntityService, CertificateEntityService>();
        services.AddScoped<IHolderEntityService, HolderEntityService>();
        services.AddScoped<ICategoryEntityService, CategoryEntityService>();
        services.AddScoped<IUserEntityService, UserEntityService>();
        services.AddScoped<ITemplateEntityService, TemplateEntityService>();
        services.AddScoped<ISignatureEntityService, SignatureEntityService>();
        services.AddScoped<ITrainingEntityService, TrainingEntityService>();
        services.AddScoped<IParticipantEntityService, ParticipantEntityService>();

        // Factories
        services.AddScoped<ICertificateCrudFactory, CertificateCrudFactory>();
        services.AddScoped<IHolderCrudFactory, HolderCrudFactory>();
        services.AddScoped<ICategoryCrudFactory, CategoryCrudFactory>();
        services.AddScoped<IAuthFactory, AuthFactory>();
        services.AddScoped<ITemplateCrudFactory, TemplateCrudFactory>();
        services.AddScoped<ISignatureCrudFactory, SignatureCrudFactory>();
        services.AddScoped<ITrainingCrudFactory, TrainingCrudFactory>();
        services.AddScoped<IParticipantCrudFactory, ParticipantCrudFactory>();

        return services;
    }
}
