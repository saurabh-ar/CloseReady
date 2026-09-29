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

    // CycleDocuments should start as an empty collection, not be required in the submitted form.
    // CycleDocuments NOT required when creating a new cycle, because those documents are created after the cycle itself
    public ICollection<CycleDocument> CycleDocuments { get; set; } = new List<CycleDocument>();
    }
  }