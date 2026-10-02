namespace CloseReady.Services
  {
  /// <summary>
  /// Defines a contract for sending emails.
  /// The controller depends on IEmailService, allowing the application
  /// to trigger email operations without knowing the underlying details
  /// of how Gmail (or any provider) actually sends the message.
  /// This abstraction enables flexibility, testability, and separation of concerns.
  /// Workflow: Controller → IEmailService → EmailService → Gmail
  /// </summary>
  public interface IEmailService
    {
    Task SendEmailAsync(string to, string subject, string body);
    }
  }