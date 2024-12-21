namespace Sandbox.FullStackIdentity.Contracts;

public record BookResponse(Guid Id, string Title, BookDetailsResponse? Details = null);

