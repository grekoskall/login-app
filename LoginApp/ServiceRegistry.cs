using LoginApp.Repositories.Common;
using LoginApp.Services;
using LoginApp.Services.Users;

public static class ServiceRegistry
{
    public static void ServicesRegister(this IServiceCollection serviceRegistry)
    {
        serviceRegistry.AddScoped<JwtAuthService, JwtAuthService>();
        serviceRegistry.AddScoped<IUserService, UserService>();
        serviceRegistry.AddScoped<MailService, MailService>();
        serviceRegistry.AddTransient<IEntityRepository, EntityRepository>();
    }
}
