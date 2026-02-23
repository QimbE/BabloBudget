using System.Data;
using Microsoft.EntityFrameworkCore;

namespace BabloBudget.Api.Repository.Resilience;

public static class ResilienceExtensions
{
    public static async Task<TResponse> ExecuteReadonlyAsync<TResponse>(
        this IDbContextFactory<ApplicationDbContext> dbContextFactory,
        Func<ApplicationDbContext, Task<TResponse>> func,
        IsolationLevel isolationLevel = IsolationLevel.Snapshot,
        int retryAttemptsCount = 6,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetriesAsync(
            retryAttemptsCount,
            () => ExecuteReadonlyAsync(dbContextFactory, func, isolationLevel, cancellationToken));
    }
    
    private static async Task<TResponse> ExecuteReadonlyAsync<TResponse>(
        this IDbContextFactory<ApplicationDbContext> dbContextFactory,
        Func<ApplicationDbContext, Task<TResponse>> func,
        IsolationLevel isolationLevel = IsolationLevel.Snapshot,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        return await func(dbContext);
    }
    
    public static async Task ExecuteAndCommitAsync(
        this IDbContextFactory<ApplicationDbContext> dbContextFactory,
        Func<ApplicationDbContext, Task> action,
        int retryAttemptsCount = 6,
        IsolationLevel isolationLevel = IsolationLevel.Snapshot,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAndCommitAsync(dbContextFactory, Func, retryAttemptsCount, isolationLevel, cancellationToken);

        return;

        async Task<int> Func(ApplicationDbContext registry)
        {
            await action(registry);
            return 0;
        }
    }
    
    public static async Task<TResult> ExecuteAndCommitAsync<TResult>(
        this IDbContextFactory<ApplicationDbContext> dbContextFactory,
        Func<ApplicationDbContext, Task<TResult>> func,
        int retryAttemptsCount = 6,
        IsolationLevel isolationLevel = IsolationLevel.Snapshot,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetriesAsync(
            retryAttemptsCount,
            () => ExecuteAndCommitAsync(dbContextFactory, func, isolationLevel, cancellationToken));
    }

    private static async Task<TResponse> ExecuteAndCommitAsync<TResponse>(
        this IDbContextFactory<ApplicationDbContext> dbContextFactory,
        Func<ApplicationDbContext, Task<TResponse>> func,
        IsolationLevel isolationLevel = IsolationLevel.Snapshot,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        
        var response = await func(dbContext);
        
        await dbContext.Database.CommitTransactionAsync(cancellationToken);
        return response;
    }
    
    private static async Task<TResponse> ExecuteWithRetriesAsync<TResponse>(int retryAttemptsCount, Func<Task<TResponse>> func)
    {
        var retryCount = 0;

        while (retryCount < retryAttemptsCount)
        {
            try
            {
                return await func();
            }
            catch (Exception exception) when (exception.IsInsertConflictException() || exception.IsChangeConflictException())
            {
                retryCount++;
            }
        }

        throw new InvalidOperationException($"Unable to complete operation after {retryAttemptsCount} retries.");
    }
    

}