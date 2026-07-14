using System.Text.Json;
using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class ImportService
{
    private readonly ImportRepository _imports;

    public ImportService(ImportRepository imports)
    {
        _imports = imports;
    }

    public async Task<string> CreateTestImportBatchAsync(string sourceFile)
    {
        var now = DateTime.UtcNow;

        var batch = new ImportBatch
        {
            ImportBatchId = Guid.NewGuid().ToString(),
            SourceFile = sourceFile,
            Status = "Preview",
            Notes = "Test import batch",
            CreatedAt = now,
        };

        await _imports.CreateBatchAsync(batch);

        await _imports.AddRowAsync(
            new ImportBatchRow
            {
                ImportBatchRowId = Guid.NewGuid().ToString(),
                ImportBatchId = batch.ImportBatchId,
                RowNumber = 1,
                EntityType = "Transaction",
                RawJson = JsonSerializer.Serialize(
                    new
                    {
                        Date = DateTime.Today,
                        BusinessName = "Farm Supply",
                        Total = 250.00m,
                        Code = "80",
                    }
                ),
                ValidationErrors = null,
                CreatedAt = now,
            }
        );

        await _imports.AddRowAsync(
            new ImportBatchRow
            {
                ImportBatchRowId = Guid.NewGuid().ToString(),
                ImportBatchId = batch.ImportBatchId,
                RowNumber = 2,
                EntityType = "Transaction",
                RawJson = JsonSerializer.Serialize(
                    new
                    {
                        Date = "",
                        BusinessName = "",
                        Total = -10,
                        Code = "UNKNOWN",
                    }
                ),
                ValidationErrors = "Missing date; total cannot be negative; unknown code",
                CreatedAt = now,
            }
        );

        return batch.ImportBatchId;
    }

    public Task<IReadOnlyList<ImportBatch>> ListBatchesAsync()
    {
        return _imports.ListBatchesAsync();
    }

    public Task<IReadOnlyList<ImportBatchRow>> ListRowsAsync(string importBatchId)
    {
        return _imports.ListRowsAsync(importBatchId);
    }
}
