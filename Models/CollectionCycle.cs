namespace CloseReady.Models
  {
  public class CollectionCycle
    {
    public int Id { get; set; }

    public int ClientId { get; set; }

    public Client? Client { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public string Status { get; set; } = "In Progress";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CycleDocument> CycleDocuments { get; set; }
    }
  }