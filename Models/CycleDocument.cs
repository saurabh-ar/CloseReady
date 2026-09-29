namespace CloseReady.Models
  {
  public class CycleDocument
    {
    public int Id { get; set; }

    public int CollectionCycleId { get; set; }

    public CollectionCycle CollectionCycle { get; set; } = null!;

    public int DocumentTypeId { get; set; }

    public DocumentType DocumentType { get; set; } = null!;

    public string Status { get; set; } = "Missing";

    public string? FileName { get; set; }

    public string? StoredFileName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
  }