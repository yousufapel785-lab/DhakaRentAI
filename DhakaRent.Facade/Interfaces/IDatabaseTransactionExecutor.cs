
namespace DhakaRent.Facade.Interfaces
{
    public interface IDatabaseTransactionExecutor
    {
        // Execute multiple database operations
        // inside a single transaction.
        //
        // All operations succeed -> Commit
        // Any operation fails -> Rollback
        //
        // Returns the result produced by the operation.

        Task<TResult> ExecuteAsync<TResult>(
            Func<Task<TResult>> operation);
    }
}
