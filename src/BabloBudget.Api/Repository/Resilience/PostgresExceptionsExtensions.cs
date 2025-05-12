using Npgsql;

namespace BabloBudget.Api.Repository.Resilience;

public static class PostgresExceptionsExtensions
{
    public static bool IsInsertConflictException(this Exception exception)
    {
        if (exception.IsUniqueIndexViolationException())
            return true;

        return exception.InnerException is not null && IsInsertConflictException(exception.InnerException);
    }

    public static bool IsChangeConflictException(this Exception exception)
    {
        if (exception.IsSnapshotUpdateConflictException())
            return true;

        return exception.InnerException is not null && IsChangeConflictException(exception.InnerException);
    }
        
    private static bool IsUniqueIndexViolationException(this Exception exception) =>
        exception is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
        

    private static bool IsSnapshotUpdateConflictException(this Exception exception) =>
        exception is PostgresException
        {
            SqlState:
            PostgresErrorCodes.SerializationFailure
            or PostgresErrorCodes.DeadlockDetected
            or PostgresErrorCodes.TransactionRollback
        };
}