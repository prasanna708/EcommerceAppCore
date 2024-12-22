using System;
using System.Collections.Generic;

namespace EcommerceAppCore.Models;

public partial class ActivityLog
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public DateTime ActivityDateAndTime { get; set; }

    public string? Action { get; set; }

    public string? TableEffected { get; set; }

    public int? TableId { get; set; }

    public string? Details { get; set; }
}
