using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altairis.Backoffice.Infrastructure.Persistence.Configurations;

internal static class TrigramIndexExtensions
{
    public static IndexBuilder<TEntity> IsTrigramIndex<TEntity>(this IndexBuilder<TEntity> index) =>
        index.HasMethod("gin").HasOperators("gin_trgm_ops");
}
