namespace Sandbox.FullStackIdentity.Contracts;

public record OrganizationResponse(string Name, string EmailLocalPart, string[] BlacklistedEmails);
