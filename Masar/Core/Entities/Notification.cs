using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class Notification
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; }

    [Required]
    [MaxLength(600)]
    public string Message { get; set; }

    [MaxLength(300)]
    public string Url { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;


    // Relations (if UserId is null notification is for admins ,else it is for a specific user)
    public int? UserId { get; set; }

    // Navigation Property 
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}
