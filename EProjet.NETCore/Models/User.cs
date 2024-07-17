using System;
using System.Collections.Generic;

namespace EProjet.NETCore.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Fullname { get; set; } = null!;

    public byte MembershipType { get; set; }

    public DateTime ExpirationDate { get; set; }

    public byte Role { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    public virtual ICollection<Tip> Tips { get; set; } = new List<Tip>();
}
