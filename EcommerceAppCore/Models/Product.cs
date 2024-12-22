using System;
using System.Collections.Generic;

namespace EcommerceAppCore.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? Name { get; set; }

    public double? Price { get; set; }

    public int? Quantity { get; set; }

    public string? Photo { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
