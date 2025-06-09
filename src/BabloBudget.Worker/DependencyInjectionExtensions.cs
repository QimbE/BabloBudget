using BabloBudget.Worker.Tasks;

namespace BabloBudget.Worker;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<IJobHostFactory, JobHostFactory>()
            .AddSingleton<IPeriodicalJobHostFactory, JobHostFactory>();
        
        services.AddSingleton<MoneyFlowJob>();
        services.AddJob<MoneyFlowJob>(TimeSpan.FromMinutes(1));
        
        return services;
    }
}