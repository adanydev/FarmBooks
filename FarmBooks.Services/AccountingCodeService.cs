using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class AccountingCodeService
{
    private readonly AccountingCodeRepository _codes;
    private readonly AuditService _auditService;

    public AccountingCodeService(
        AccountingCodeRepository codes,
        AuditService auditService)
    {
        _codes = codes;
        _auditService = auditService;
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

        await _auditService.WriteAsync(
            "AccountingCode",
            code.CodeId,
            "Created",
            null,
            code);

        return code.CodeId;
    }

    public Task<IReadOnlyList<AccountingCode>> ListActiveCodesAsync()
    {
        return _codes.ListActiveAsync();
    }

    public Task<IReadOnlyList<AccountingCode>> ListAllCodesAsync()
    {
        return _codes.ListAllAsync();
    }

    public async Task UpdateCodeAsync(
        string codeId,
        string codeValue,
        string name,
        string? description,
        bool isActive)
    {
        var code = await _codes.GetAsync(codeId);

        if (code is null)
            throw new InvalidOperationException("Accounting code not found.");

        if (string.IsNullOrWhiteSpace(codeValue))
            throw new InvalidOperationException("Code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Code name is required.");

        var oldCode = new AccountingCode
        {
            CodeId = code.CodeId,
            Code = code.Code,
            Name = code.Name,
            Description = code.Description,
            IsActive = code.IsActive,
            CreatedAt = code.CreatedAt,
            UpdatedAt = code.UpdatedAt
        };

        code.Code = codeValue.Trim();
        code.Name = name.Trim();
        code.Description = description;
        code.IsActive = isActive;

        await _codes.UpdateAsync(code);

        await _auditService.WriteAsync(
            "AccountingCode",
            code.CodeId,
            "Updated",
            oldCode,
            code);
    }

    public async Task DisableCodeAsync(string codeId)
    {
        var code = await _codes.GetAsync(codeId);

        if (code is null)
            throw new InvalidOperationException("Accounting code not found.");

        await _codes.DisableAsync(codeId);

        await _auditService.WriteAsync(
            "AccountingCode",
            codeId,
            "Disabled",
            code,
            new { CodeId = codeId, IsActive = false });
    }

    public async Task ReactivateCodeAsync(string codeId)
    {
        var code = await _codes.GetAsync(codeId);

        if (code is null)
            throw new InvalidOperationException("Accounting code not found.");

        await _codes.ReactivateAsync(codeId);

        await _auditService.WriteAsync(
            "AccountingCode",
            codeId,
            "Reactivated",
            code,
            new { CodeId = codeId, IsActive = true });
    }
}