using System.ComponentModel.DataAnnotations;

namespace FriendFace.ViewModels;

public class VerifyTwoFactorViewModel
{
    public string UserId { get; set; }

    [Required]
    [Display(Name = "2FA Code")]
    public string Token { get; set; }
}