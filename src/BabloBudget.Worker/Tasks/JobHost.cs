namespace BabloBudget.Worker.Tasks;

public class JobHost(IJob job, ILogger logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting job host for {jobName}", job.GetType().Name);

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
                logger.LogCritical(e, "Failed to execute job {jobName}", job.GetType().Name);
            }
        }
        
        logger.LogInformation("Finishing job host for {jobName}", job.GetType().Name);
    }
}

public sealed class PeriodicalJobHost(TimeSpan period, IJob job, ILogger logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting periodical job host for {jobName}", job.GetType().Name);

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
                logger.LogCritical(e, "Failed to execute job {jobName}", job.GetType().Name);
            }
            
            logger.LogInformation("Waiting for next periodical job host iteration for {jobName}", job.GetType().Name);
            await Task.Delay(period, stoppingToken); // won't work on app restart i.e., waiting at most 'period'
        }
        
        logger.LogInformation("Finishing periodical job host for {jobName}", job.GetType().Name);
    }
}

public interface IJobHostFactory
{
    BackgroundService CreateJob(IJob task);
}

public interface IPeriodicalJobHostFactory
{
    BackgroundService CreateJob(IJob task, TimeSpan period);
}

public class JobHostFactory(ILogger<JobHost> logger) : IJobHostFactory, IPeriodicalJobHostFactory
{
    public BackgroundService CreateJob(IJob job) =>
        new JobHost(job, logger);

    public BackgroundService CreateJob(IJob task, TimeSpan period) =>
        new PeriodicalJobHost(period, task, logger);
}

public interface IJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}

public static class JobHostExtensions
{
    public static IServiceCollection AddJob<TJob>(
        this IServiceCollection services) where TJob : IJob =>
        services.AddSingleton<IHostedService>(
            provider => provider
                .GetRequiredService<IJobHostFactory>()
                .CreateJob(provider.GetRequiredService<TJob>()));
    
    public static IServiceCollection AddJob<TJob>(
        this IServiceCollection services,
        TimeSpan period) where TJob : IJob =>
        services.AddSingleton<IHostedService>(
            provider => provider
                .GetRequiredService<IPeriodicalJobHostFactory>()
                .CreateJob(provider.GetRequiredService<TJob>(), period));
}