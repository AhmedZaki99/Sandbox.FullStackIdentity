using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Contracts;

public record BookDetailsResponse(
    string Author,
    string? Publisher, 
    DateTime? PublishDate,
    PublicationStatus? PublicationStatus,
    int? PagesCount);

