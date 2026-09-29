namespace CloseReady.Models
  {
  public class ClientUploadLink
    {
    public int Id { get; set; }

    public int CollectionCycleId { get; set; }

    public CollectionCycle CollectionCycle { get; set; } = null!;

    public string Token { get; set; } = Guid.NewGuid().ToString();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
    }
  }