namespace BabloBudget.Worker.Tasks;

public class JobHost(IJob job, ILogger logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting job host for {jobName}", typeof(job).Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await job.ExecuteAsync(stoppingToken);
            }
            catch (TaskCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception e)
            {
                logger.LogCritical(e, "Failed to execute job {jobName}", typeof(job).Name);
            }
        }
        
        logger.LogInformation("Finishing job host for {jobName}", typeof(job).Name);
    }
}

public interface IJobHostFactory
{
    BackgroundService CreateJob(IJob task);
}

public class JobHostFactory(ILogger<JobHost> logger) : IJobHostFactory
{
    public BackgroundService CreateJob(IJob job) =>
        new JobHost(job, logger);
}

public interface IJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}

public static class JobHostExtensions
{
    private static IServiceCollection AddJob<TJob>(
        this IServiceCollection services) where TJob : IJob =>
        services.AddSingleton<IHostedService>(
            provider => provider
                .GetRequiredService<IJobHostFactory>()
                .CreateJob(provider.GetRequiredService<TJob>()));
}