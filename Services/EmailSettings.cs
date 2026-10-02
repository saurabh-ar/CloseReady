namespace CloseReady.Services
  {
  /// <summary>
  /// EmailSettings holds the details (like server, port, and sender info)
  /// that we read from appsettings.json. 
  /// Think of it as a box where your email setup values are stored.
  /// </summary>
  public class EmailSettings
    {
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    }
  }