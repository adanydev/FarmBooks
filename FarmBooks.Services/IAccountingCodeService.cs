using FarmBooks.Core.Models;

namespace FarmBooks.Services;

public interface IAccountingCodeService
{
    Task<string> CreateCodeAsync(string codeValue, string name, string? description = null);

    Task<IReadOnlyList<AccountingCode>> ListActiveCodesAsync();

    Task<IReadOnlyList<AccountingCode>> ListAllCodesAsync();

    Task UpdateCodeAsync(
        string codeId,
        string codeValue,
        string name,
        string? description,
        bool isActive
    );

    Task DeleteCodeAsync(string codeId);
}
