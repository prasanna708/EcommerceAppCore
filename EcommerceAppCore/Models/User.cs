using System;
using System.Collections.Generic;

namespace EcommerceAppCore.Models;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public string Pwd { get; set; } = null!;

    public string Role { get; set; } = null!;

    public long MobileNumber { get; set; }

    public string Email { get; set; } = null!;

    public DateTime Dob { get; set; }

    public string Location { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
