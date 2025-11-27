using System.Net;
using System.Net.Mail;

namespace ECommerceMVC.Services
{
	public interface IEmailService
	{
		Task SendEmailAsync(string toEmail, string subject, string body);
		Task SendOTPEmailAsync(string toEmail, string otp, string fullName);
	}

	public class EmailService : IEmailService
	{
		private readonly IConfiguration _configuration;

		public EmailService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			var smtpHost = _configuration["EmailSettings:SmtpHost"];
			var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
			var senderEmail = _configuration["EmailSettings:SenderEmail"];
			var senderName = _configuration["EmailSettings:SenderName"];
			var username = _configuration["EmailSettings:Username"];
			var password = _configuration["EmailSettings:Password"];

			using var client = new SmtpClient(smtpHost, smtpPort);
			client.EnableSsl = true;
			client.Credentials = new NetworkCredential(username, password);

			var message = new MailMessage();
			message.From = new MailAddress(senderEmail!, senderName);
			message.Subject = subject;
			message.Body = body;
			message.IsBodyHtml = true;
			message.To.Add(toEmail);

			await client.SendMailAsync(message);
		}

		public async Task SendOTPEmailAsync(string toEmail, string otp, string fullName)
		{
			var subject = "Xác thực tài khoản FoodHub - Mã OTP";
			var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #FF9500 0%, #FF7C1F 100%); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 40px; }}
        .otp-box {{ background: #FFF4E6; border: 2px dashed #FF9500; border-radius: 10px; padding: 20px; text-align: center; margin: 30px 0; }}
        .otp-code {{ font-size: 36px; font-weight: bold; color: #FF9500; letter-spacing: 8px; }}
        .footer {{ background: #f8f8f8; padding: 20px; text-align: center; color: #666; font-size: 14px; }}
        .button {{ display: inline-block; background: #FF9500; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🍔 FoodHub</h1>
            <p>Xác thực tài khoản của bạn</p>
        </div>
        <div class='content'>
            <h2>Xin chào {fullName}!</h2>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại FoodHub. Để hoàn tất quá trình đăng ký, vui lòng sử dụng mã OTP dưới đây:</p>
            
            <div class='otp-box'>
                <p style='margin: 0; color: #666; font-size: 14px;'>MÃ XÁC THỰC OTP</p>
                <div class='otp-code'>{otp}</div>
                <p style='margin: 10px 0 0 0; color: #999; font-size: 13px;'>Mã có hiệu lực trong 5 phút</p>
            </div>
            
            <p style='color: #666;'>Nếu bạn không thực hiện đăng ký này, vui lòng bỏ qua email này.</p>
            
            <p style='margin-top: 30px;'>Trân trọng,<br><strong>Đội ngũ FoodHub</strong></p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>© 2025 FoodHub. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

			await SendEmailAsync(toEmail, subject, body);
		}
	}
}
