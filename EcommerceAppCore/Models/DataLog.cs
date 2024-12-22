using System;
using System.Collections.Generic;

namespace EcommerceAppCore.Models;

public partial class DataLog
{
    public int Id { get; set; }

    public string? TableEffected { get; set; }

    public int? PropertyId { get; set; }

    public string? PropertyEffected { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? UserId { get; set; }

    public DateTime ActivityDateAndTime { get; set; }
}
