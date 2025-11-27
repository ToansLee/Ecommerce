using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Models;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using ECommerceMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceMVC.Controllers
{
	public class KhachHangController : Controller
	{
		private readonly FoodOrderingContext db;
		private readonly IMapper _mapper;

		public KhachHangController(FoodOrderingContext context, IMapper mapper)
		{
			db = context;
			_mapper = mapper;
		}

		#region Đăng ký
		[HttpGet]
		public IActionResult DangKy()
		{
			return View();
		}

	[HttpPost]
	public async Task<IActionResult> DangKy(RegisterVM model, [FromServices] IEmailService emailService)
	{
		if (ModelState.IsValid)
		{
			try
			{
			// Kiểm tra username đã tồn tại chưa
			if (db.Customers.Any(kh => kh.UserName == model.MaKh))
			{
				ModelState.AddModelError("MaKh", "Tên đăng nhập này đã được sử dụng");
				return View(model);
			}
			
			// Kiểm tra email đã tồn tại chưa
			if (db.Customers.Any(kh => kh.Email == model.Email))
			{
				ModelState.AddModelError("Email", "Email này đã được sử dụng");
				return View(model);
			}

			// Tạo mã OTP 6 số
			var random = new Random();
			var otp = random.Next(100000, 999999).ToString();
			var otpExpiry = DateTime.Now.AddMinutes(5);

			// Lưu thông tin tạm thời vào Session (chưa lưu vào database)
			HttpContext.Session.SetString("PendingRegistration", System.Text.Json.JsonSerializer.Serialize(model));
			HttpContext.Session.SetString("RegistrationOTP", otp);
			HttpContext.Session.SetString("OTPExpiry", otpExpiry.ToString("o"));

			// Gửi email OTP
			try
			{
				await emailService.SendOTPEmailAsync(model.Email, otp, model.HoTen);
				TempData["Email"] = model.Email;
				TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng kiểm tra email để lấy mã OTP.";
				return RedirectToAction("VerifyOTP");
			}
			catch (Exception)
			{
				// Xóa thông tin session nếu gửi email thất bại
				HttpContext.Session.Remove("PendingRegistration");
				HttpContext.Session.Remove("RegistrationOTP");
				HttpContext.Session.Remove("OTPExpiry");
				ModelState.AddModelError("", "Không thể gửi email xác thực. Vui lòng thử lại sau.");
			}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
			}
		}
		return View(model);
	}
	#endregion

	#region Xác thực OTP
		[HttpGet]
		public IActionResult VerifyOTP()
		{
			if (TempData["Email"] == null)
			{
				return RedirectToAction("DangKy");
			}
			
			ViewBag.Email = TempData["Email"];
			TempData.Keep("Email");
			return View();
		}

	[HttpPost]
	public IActionResult VerifyOTP(string email, string otp)
	{
		// Lấy thông tin từ Session
		var pendingRegistrationJson = HttpContext.Session.GetString("PendingRegistration");
		var sessionOTP = HttpContext.Session.GetString("RegistrationOTP");
		var otpExpiryString = HttpContext.Session.GetString("OTPExpiry");

		if (string.IsNullOrEmpty(pendingRegistrationJson) || string.IsNullOrEmpty(sessionOTP))
		{
			ViewBag.ErrorMessage = "Phiên đăng ký đã hết hạn. Vui lòng đăng ký lại.";
			ViewBag.Email = email;
			return View();
		}

		var registrationData = System.Text.Json.JsonSerializer.Deserialize<RegisterVM>(pendingRegistrationJson);

		if (registrationData == null || registrationData.Email != email)
		{
			ViewBag.ErrorMessage = "Email không khớp";
			ViewBag.Email = email;
			return View();
		}

		if (sessionOTP != otp)
		{
			ViewBag.ErrorMessage = "Mã OTP không chính xác";
			ViewBag.Email = email;
			return View();
		}

		if (!string.IsNullOrEmpty(otpExpiryString) && DateTime.Parse(otpExpiryString) < DateTime.Now)
		{
			ViewBag.ErrorMessage = "Mã OTP đã hết hạn";
			ViewBag.Email = email;
			return View();
		}

		// Xác thực thành công - LƯU VÀO DATABASE
		try
		{
			// Kiểm tra lại username và email chưa bị trùng (trong trường hợp có người đăng ký cùng lúc)
			if (db.Customers.Any(kh => kh.UserName == registrationData.MaKh))
			{
				ViewBag.ErrorMessage = "Tên đăng nhập đã được sử dụng. Vui lòng đăng ký lại với tên khác.";
				ViewBag.Email = email;
				// Xóa session
				HttpContext.Session.Remove("PendingRegistration");
				HttpContext.Session.Remove("RegistrationOTP");
				HttpContext.Session.Remove("OTPExpiry");
				return View();
			}

			if (db.Customers.Any(kh => kh.Email == registrationData.Email))
			{
				ViewBag.ErrorMessage = "Email đã được sử dụng. Vui lòng đăng ký lại với email khác.";
				ViewBag.Email = email;
				// Xóa session
				HttpContext.Session.Remove("PendingRegistration");
				HttpContext.Session.Remove("RegistrationOTP");
				HttpContext.Session.Remove("OTPExpiry");
				return View();
			}

		var khachHang = _mapper.Map<Customer>(registrationData);
		khachHang.CreatedAt = DateTime.Now;
		khachHang.IsActive = true;

		db.Add(khachHang);
			db.SaveChanges();

			// Xóa thông tin session
			HttpContext.Session.Remove("PendingRegistration");
			HttpContext.Session.Remove("RegistrationOTP");
			HttpContext.Session.Remove("OTPExpiry");

			TempData["SuccessMessage"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";
			return RedirectToAction("DangNhap");
		}
		catch (Exception ex)
		{
			ViewBag.ErrorMessage = $"Có lỗi xảy ra khi tạo tài khoản: {ex.Message}";
			ViewBag.Email = email;
			return View();
		}
	}	[HttpPost]
	public async Task<IActionResult> ResendOTP(string email, [FromServices] IEmailService emailService)
	{
		// Lấy thông tin từ Session
		var pendingRegistrationJson = HttpContext.Session.GetString("PendingRegistration");

		if (string.IsNullOrEmpty(pendingRegistrationJson))
		{
			return Json(new { success = false, message = "Phiên đăng ký đã hết hạn. Vui lòng đăng ký lại." });
		}

		var registrationData = System.Text.Json.JsonSerializer.Deserialize<RegisterVM>(pendingRegistrationJson);

		if (registrationData == null || registrationData.Email != email)
		{
			return Json(new { success = false, message = "Email không khớp" });
		}

		// Tạo OTP mới
		var random = new Random();
		var otp = random.Next(100000, 999999).ToString();
		var otpExpiry = DateTime.Now.AddMinutes(5);

		// Cập nhật Session
		HttpContext.Session.SetString("RegistrationOTP", otp);
		HttpContext.Session.SetString("OTPExpiry", otpExpiry.ToString("o"));

		// Gửi email OTP
		try
		{
			await emailService.SendOTPEmailAsync(registrationData.Email, otp, registrationData.HoTen);
			return Json(new { success = true, message = "Đã gửi lại mã OTP. Vui lòng kiểm tra email." });
		}
		catch (Exception)
		{
			return Json(new { success = false, message = "Không thể gửi email. Vui lòng thử lại sau." });
		}
	}
	#endregion

	#region Đăng nhập
		[HttpGet]
		public IActionResult DangNhap(string? returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> DangNhap(LoginVM model, string? returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			
			if (ModelState.IsValid)
			{
				// Tìm theo username hoặc email
				var khachHang = db.Customers.SingleOrDefault(kh => kh.UserName == model.UserName || kh.Email == model.UserName);
				
				if (khachHang == null)
				{
					ViewBag.ErrorMessage = "Tên đăng nhập hoặc email không tồn tại";
				}
				else
				{
					// So sánh mật khẩu đã hash
					if (khachHang.PasswordHash != model.Password)
					{
						ViewBag.ErrorMessage = "Mật khẩu không đúng";
					}
					else if (!khachHang.IsActive)
					{
						ViewBag.ErrorMessage = "Tài khoản đã bị khóa";
					}
					else
					{
						// Đăng nhập thành công
						var claims = new List<Claim>
						{
							new Claim(ClaimTypes.Email, khachHang.Email),
							new Claim(ClaimTypes.Name, khachHang.FullName),
							new Claim(ClaimTypes.NameIdentifier, khachHang.Id.ToString()),
							new Claim(ClaimTypes.Role, khachHang.Role)
						};

						var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
						var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

						await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal,
							new AuthenticationProperties
							{
								IsPersistent = model.RememberMe
							});

						// Redirect theo role
						if (Url.IsLocalUrl(returnUrl))
						{
							return Redirect(returnUrl);
						}
						else if (khachHang.Role == "Seller")
						{
							return RedirectToAction("Index", "Seller");
						}
						else
						{
							return Redirect("/");
						}
					}
				}
			}
			return View(model);
		}
		#endregion

		#region Đăng xuất
		[HttpGet]
		public async Task<IActionResult> DangXuat()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return Redirect("/");
		}
		#endregion

		#region Thông tin tài khoản
		[HttpGet]
		public async Task<IActionResult> Profile()
		{
			if (User.Identity?.IsAuthenticated != true)
			{
				return RedirectToAction("DangNhap");
			}

		var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
		var customer = await db.Customers
			.FirstOrDefaultAsync(c => c.Id == userId);

		if (customer == null)
			{
				await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
				return RedirectToAction("DangNhap");
			}

			return View(customer);
		}

		[HttpGet]
		public async Task<IActionResult> EditProfile()
		{
			if (User.Identity?.IsAuthenticated != true)
			{
				return RedirectToAction("DangNhap");
			}

			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var customer = await db.Customers.FindAsync(userId);

			if (customer == null)
			{
				return RedirectToAction("DangNhap");
			}

			var model = new EditProfileVM
			{
				FullName = customer.FullName,
				Email = customer.Email,
				Phone = customer.Phone ?? string.Empty
			};

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> EditProfile(EditProfileVM model)
		{
			if (User.Identity?.IsAuthenticated != true)
			{
				return RedirectToAction("DangNhap");
			}

			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			
			if (ModelState.IsValid)
			{
				try
				{
					// Kiểm tra email mới có trùng với người khác không
					if (await db.Customers.AnyAsync(c => c.Email == model.Email && c.Id != userId))
					{
						ModelState.AddModelError("Email", "Email này đã được sử dụng bởi tài khoản khác");
						return View(model);
					}

					var customer = await db.Customers.FindAsync(userId);
					if (customer == null)
					{
						return RedirectToAction("DangNhap");
					}

					customer.FullName = model.FullName;
					customer.Email = model.Email;
					customer.Phone = model.Phone;

					await db.SaveChangesAsync();

					// Cập nhật lại Claims với tên mới
					await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

					var claims = new List<Claim>
					{
						new Claim(ClaimTypes.Email, customer.Email),
						new Claim(ClaimTypes.Name, customer.FullName),
						new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
						new Claim(ClaimTypes.Role, customer.Role)
					};

					var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
					var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

					await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

					TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
					return RedirectToAction("Profile");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
				}
			}

			return View(model);
		}

		[HttpGet]
		public IActionResult ChangePassword()
		{
			if (User.Identity?.IsAuthenticated != true)
			{
				return RedirectToAction("DangNhap");
			}

			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
		{
			if (User.Identity?.IsAuthenticated != true)
			{
				return RedirectToAction("DangNhap");
			}

			if (ModelState.IsValid)
			{
				var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
				var customer = await db.Customers.FindAsync(userId);

				if (customer == null)
				{
					return RedirectToAction("DangNhap");
				}

				// Kiểm tra mật khẩu cũ
				if (customer.PasswordHash != model.OldPassword)
				{
					ModelState.AddModelError("OldPassword", "Mật khẩu cũ không đúng");
					return View(model);
				}

				// Cập nhật mật khẩu mới
				customer.PasswordHash = model.NewPassword;
				await db.SaveChangesAsync();

				TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
				return RedirectToAction("Profile");
			}

			return View(model);
		}
		#endregion

		#region Quên mật khẩu
		[HttpGet]
		public IActionResult QuenMatKhau()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> QuenMatKhau(string email, [FromServices] IEmailService emailService)
		{
			if (string.IsNullOrEmpty(email))
			{
				ModelState.AddModelError("", "Vui lòng nhập email");
				return View();
			}

			// Kiểm tra email có tồn tại trong hệ thống không
			var customer = await db.Customers.FirstOrDefaultAsync(c => c.Email == email);

			if (customer == null)
			{
				ModelState.AddModelError("", "Email không tồn tại trong hệ thống");
				return View();
			}

			try
			{
				// Gửi mật khẩu về email
				string subject = "Khôi phục mật khẩu - FoodHub";
				string body = $@"
					<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
						<div style='background: linear-gradient(135deg, #FF9500 0%, #FF7C1F 100%); padding: 20px; text-align: center;'>
							<h1 style='color: white; margin: 0;'>🍔 FoodHub</h1>
						</div>
						<div style='padding: 30px; background-color: #f9f9f9;'>
							<h2 style='color: #FF9500;'>Khôi phục mật khẩu</h2>
							<p>Xin chào <strong>{customer.FullName}</strong>,</p>
							<p>Bạn đã yêu cầu khôi phục mật khẩu. Dưới đây là thông tin đăng nhập của bạn:</p>
							<div style='background-color: white; padding: 20px; border-radius: 10px; border-left: 4px solid #FF9500; margin: 20px 0;'>
								<p style='margin: 5px 0;'><strong>Tên đăng nhập:</strong> {customer.UserName}</p>
								<p style='margin: 5px 0;'><strong>Mật khẩu:</strong> {customer.PasswordHash}</p>
							</div>
							<p style='color: #dc3545; font-weight: 500;'>
								<i>⚠️ Vui lòng đổi mật khẩu sau khi đăng nhập để bảo mật tài khoản!</i>
							</p>
							<p>Nếu bạn không yêu cầu khôi phục mật khẩu, vui lòng bỏ qua email này.</p>
							<hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
							<p style='color: #666; font-size: 12px;'>Email này được gửi tự động, vui lòng không trả lời.</p>
						</div>
						<div style='background-color: #333; padding: 15px; text-align: center; color: white; font-size: 12px;'>
							<p style='margin: 0;'>© 2024 FoodHub. All rights reserved.</p>
							<p style='margin: 5px 0;'>218 Lĩnh Nam, Hoàng Mai, Hà Nội</p>
						</div>
					</div>
				";

				await emailService.SendEmailAsync(email, subject, body);

				TempData["SuccessMessage"] = "Mật khẩu đã được gửi về email của bạn. Vui lòng kiểm tra hộp thư!";
				return RedirectToAction("DangNhap");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"Có lỗi xảy ra khi gửi email: {ex.Message}");
				return View();
			}
		}
		#endregion
	}
}
