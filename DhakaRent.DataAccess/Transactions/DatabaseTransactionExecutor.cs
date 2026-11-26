
using DhakaRent.DataAccess.Context;
using DhakaRent.Facade.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DhakaRent.DataAccess.Transactions
{
    public class DatabaseTransactionExecutor
        : IDatabaseTransactionExecutor
    {
        private readonly ApplicationDbContext _context;

        public DatabaseTransactionExecutor(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TResult> ExecuteAsync<TResult>(
            Func<Task<TResult>> operation)
        {
            ArgumentNullException.ThrowIfNull(operation);

            // Support EF Core's configured retry strategy.
            var strategy = _context.Database
                .CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // Start a database transaction.
                await using var transaction =
                    await _context.Database
                        .BeginTransactionAsync();

                try
                {
                    // Execute the provided database operations.
                    TResult result = await operation();

                    // Save any remaining tracked changes.
                    await _context.SaveChangesAsync();

                    // Commit only after all operations succeed.
                    await transaction.CommitAsync();

                    return result;
                }
                catch
                {
                    // Roll back if any operation fails.
                    await transaction.RollbackAsync();

                    throw;
                }
            });
        }
    }
}
