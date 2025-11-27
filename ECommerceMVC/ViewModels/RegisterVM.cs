using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
	public class RegisterVM
	{
		[Key]
		[Display(Name = "T�n dang nh?p")]
		[Required(ErrorMessage = "*")]
		[MaxLength(20, ErrorMessage = "T?i da 20 k� t?")]
		public string MaKh { get; set; }


		[Display(Name ="M?t kh?u")]
		[Required(ErrorMessage = "*")]
		[DataType(DataType.Password)]
		public string MatKhau { get; set; }

		[Display(Name ="H? t�n")]
		[Required(ErrorMessage = "*")]
		[MaxLength(50, ErrorMessage = "T?i da 50 k� t?")]
		public string HoTen { get; set; }

		public bool GioiTinh { get; set; } = true;

		[Display(Name ="Ng�y sinh")]
		[DataType(DataType.Date)]
		public DateTime? NgaySinh { get; set; }

		[Display(Name ="�?a ch?")]
		[MaxLength(60, ErrorMessage = "T?i da 60 k� t?")]
		public string DiaChi { get; set; }

	[Display(Name = "�i?n tho?i")]
	[MaxLength(24, ErrorMessage = "T?i da 24 k� t?")]
	[RegularExpression(@"0[9875]\d{8}", ErrorMessage ="Chua d�ng d?nh d?ng di d?ng Vi?t Nam")]
	public string DienThoai { get; set; }

	[Display(Name = "Vai tr�")]
	[Required(ErrorMessage = "Vui l�ng ch?n vai tr�")]
	public string Role { get; set; } = "Customer";

	[EmailAddress(ErrorMessage ="Chua d�ng d?nh d?ng email")]
	public string Email { get; set; }
	}
}
