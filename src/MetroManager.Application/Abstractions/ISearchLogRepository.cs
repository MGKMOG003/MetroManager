using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetroManager.Application.Abstractions
{
    public record SearchQueryDto(
        IReadOnlyCollection<string> Categories,
        DateTime? FromUtc,
        DateTime? ToUtc,
        string ClientFingerprint,
        DateTime OccurredUtc,
        string? UserId
    );

    public interface ISearchLogRepository
    {
        Task LogAsync(SearchQueryDto query);
    }
}
