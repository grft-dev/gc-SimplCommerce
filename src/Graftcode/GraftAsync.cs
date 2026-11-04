using System;
using System.Threading.Tasks;

namespace SimplCommerce.Graftcode;

internal static class GraftAsync
{
    // Graft methods are synchronous for now. Keep async repository and EF calls,
    // and bridge to sync only at the public boundary until GG supports Task.
    internal static T Run<T>(Func<Task<T>> operation) =>
        Task.Run(operation).ConfigureAwait(false).GetAwaiter().GetResult();
}
