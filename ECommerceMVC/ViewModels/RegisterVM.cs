using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
	public class RegisterVM
	{
		[Key]
		[Display(Name = "Tên đăng nhập")]
		[Required(ErrorMessage = "*")]
		[MaxLength(20, ErrorMessage = "Tối đa 20 ký tự")]
		public string MaKh { get; set; }


		[Display(Name ="Mật khẩu")]
		[Required(ErrorMessage = "*")]
		[DataType(DataType.Password)]
		public string MatKhau { get; set; }

		[Display(Name ="Họ tên")]
		[Required(ErrorMessage = "*")]
		[MaxLength(50, ErrorMessage = "Tối đa 50 ký tự")]
		public string HoTen { get; set; }

		public bool GioiTinh { get; set; } = true;

		[Display(Name ="Ngày sinh")]
		[DataType(DataType.Date)]
		public DateTime? NgaySinh { get; set; }

		[Display(Name ="Địa chỉ")]
		[MaxLength(60, ErrorMessage = "Tối đa 60 ký tự")]
		public string DiaChi { get; set; }

	[Display(Name = "Điện thoại")]
	[MaxLength(24, ErrorMessage = "Tối đa 24 ký tự")]
	[RegularExpression(@"0[9875]\d{8}", ErrorMessage ="Chưa đúng định dạng di động Việt Nam")]
	public string DienThoai { get; set; }

	[Display(Name = "Vai trò")]
	[Required(ErrorMessage = "Vui lòng chọn vai trò")]
	public string Role { get; set; } = "Customer";

	[EmailAddress(ErrorMessage ="Chưa đúng định dạng email")]
	public string Email { get; set; }
	}
}
