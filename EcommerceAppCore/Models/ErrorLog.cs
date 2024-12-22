using System;
using System.Collections.Generic;

namespace EcommerceAppCore.Models;

public partial class ErrorLog
{
    public int ErrorId { get; set; }

    public string? UserId { get; set; }

    public string? ErrorActionMethod { get; set; }

    public int? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public string? StackTrace { get; set; }

    public DateTime DateAndTime { get; set; }
}
