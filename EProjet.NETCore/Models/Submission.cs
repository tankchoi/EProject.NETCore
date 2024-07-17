using System;
using System.Collections.Generic;

namespace EProjet.NETCore.Models;

public partial class Submission
{
    public int Id { get; set; }

    public int? CompetitionId { get; set; }

    public int? RecipeId { get; set; }

    public int? TipId { get; set; }

    public DateTime Date { get; set; }

    public bool? IsWinner { get; set; }

    public virtual Competition? Competition { get; set; }

    public virtual Recipe? Recipe { get; set; }

    public virtual Tip? Tip { get; set; }
}
