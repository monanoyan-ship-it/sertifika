using Sertifika.EntityServices;
using Sertifika.Factories.Categories;
using Sertifika.Factories.Certificates;
using Sertifika.Factories.Holders;
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

        // Factories
        services.AddScoped<ICertificateCrudFactory, CertificateCrudFactory>();
        services.AddScoped<IHolderCrudFactory, HolderCrudFactory>();
        services.AddScoped<ICategoryCrudFactory, CategoryCrudFactory>();

        return services;
    }
}
