namespace Sandbox.FullStackIdentity.Contracts;

public record BookResponse(Guid Id, string Title, Guid? CreatorId, string? SenderEmail, BookDetailsResponse? Details = null);

