using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
	public class LoginVM
	{
		[Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
	[Display(Name = "Tên đăng nhập hoặc Email")]
	public string UserName { get; set; }		[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
		[Display(Name = "Mật khẩu")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Display(Name = "Ghi nhớ đăng nhập")]
		public bool RememberMe { get; set; }
	}
}
