namespace FarmBooks.Core.Models;

public sealed class TransactionDocument
{
    public string TransactionDocumentId { get; set; } = "";
    public string TransactionId { get; set; } = "";
    public string FileName { get; set; } = "";
    public string MimeType { get; set; } = "";
    public byte[] DocumentBlob { get; set; } = [];
    public byte[]? ThumbnailBlob { get; set; }
    public string DocumentType { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}