using System;
using System.Collections.Generic;

namespace InsuranceQuoter_Repository.Models;

public partial class State
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; } = new List<User>();
}
