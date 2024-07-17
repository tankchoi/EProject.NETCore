using System;
using System.Collections.Generic;

namespace EProjet.NETCore.Models;

public partial class Competition
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
