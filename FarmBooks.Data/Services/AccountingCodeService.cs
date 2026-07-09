using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Data.Services;

public sealed class AccountingCodeService
{
    private readonly AccountingCodeRepository _codes;

    public AccountingCodeService(AccountingCodeRepository codes)
    {
        _codes = codes;
    }

    public async Task<string> CreateCodeAsync(
        string codeValue,
        string name,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(codeValue))
            throw new InvalidOperationException("Code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Code name is required.");

        var now = DateTime.UtcNow;

        var code = new AccountingCode
        {
            CodeId = Guid.NewGuid().ToString(),
            Code = codeValue.Trim(),
            Name = name.Trim(),
            Description = description,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _codes.CreateAsync(code);

        return code.CodeId;
    }

    public Task<IReadOnlyList<AccountingCode>> ListActiveCodesAsync()
    {
        return _codes.ListActiveAsync();
    }
}