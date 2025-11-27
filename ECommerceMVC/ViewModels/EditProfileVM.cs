using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
	public class EditProfileVM
	{
		[Required(ErrorMessage = "Vui lòng nhập họ tên")]
		[Display(Name = "Họ tên")]
		public string FullName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập email")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		[Display(Name = "Email")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
		[Display(Name = "Số điện thoại")]
		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		public string Phone { get; set; } = string.Empty;
	}

	public class ChangePasswordVM
	{
		[Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
		[Display(Name = "Mật khẩu cũ")]
		[DataType(DataType.Password)]
		public string OldPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
		[Display(Name = "Mật khẩu mới")]
		[DataType(DataType.Password)]
		[MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
		public string NewPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
		[Display(Name = "Xác nhận mật khẩu mới")]
		[DataType(DataType.Password)]
		[Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
