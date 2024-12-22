using System;
using System.Collections.Generic;

namespace EcommerceAppCore.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? ProductId { get; set; }

    public string? UserId { get; set; }

    public string? Name { get; set; }

    public int? Quantity { get; set; }

    public DateTime OrderDateAndTime { get; set; }

    public double? TotalPrice { get; set; }

    public string? Photo { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
