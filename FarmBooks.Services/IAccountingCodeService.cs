using FarmBooks.Core.Models;

namespace FarmBooks.Services;

public interface IAccountingCodeService
{
    Task<IReadOnlyList<AccountingCode>> ListActiveCodesAsync();
    Task<IReadOnlyList<AccountingCode>> ListAllCodesAsync();
}