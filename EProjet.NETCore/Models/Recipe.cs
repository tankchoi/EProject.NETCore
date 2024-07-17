using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EProjet.NETCore.Models;

public partial class Recipe
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Tên không được để trống")]
    public string Fullname { get; set; } = null!;
    [Required(ErrorMessage = "Email không được để trống")]
    public string Email { get; set; } = null!;

    public string Img { get; set; } = null!;

    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    public string Title { get; set; } = null!;

    
    public string Content { get; set; } = null!;

    public byte Type { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual User? User { get; set; }
}
