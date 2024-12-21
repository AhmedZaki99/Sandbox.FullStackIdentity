using System.ComponentModel.DataAnnotations;

namespace Sandbox.FullStackIdentity.Contracts;

public sealed class SchedulingRequest
{

    [Required(AllowEmptyStrings = false)]
    public required string Cron { get; set; }
}
