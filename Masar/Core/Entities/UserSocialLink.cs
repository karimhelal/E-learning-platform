using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class UserSocialLink
{
    [Key]
    [Column("social_link_id")]
    [Display(Name = "User Social Link ID")]
    public int UserSocialLinkId { get; set; }


    [Required(ErrorMessage = "{0} is required and can't be null.")]
    [Column("url")]
    [Display(Name = "Social URL")]
    [Url(ErrorMessage = "{0} is not a valid url.")]
    public string Url { get; set; }


    [Required]
    [Column("platform")]
    public SocialPlatform SocialPlatform { get; set; } = SocialPlatform.Personal;



    [Required]
    [Column("user_id")]
    [Display(Name = "User ID")]
    public int UserId { get; set; }



    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}
