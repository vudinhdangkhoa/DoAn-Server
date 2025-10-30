using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace server.Services
{
    public class Mail_Services
    {

        private static readonly Dictionary<string, (string otp, DateTime expiry)> _otpStorage = new();

        public string GenerateOTP(string userKey) // userKey có thể là email hoặc userId
        {
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();
            var expiry = DateTime.Now.AddMinutes(10);

            // Lưu OTP theo từng user
            _otpStorage[userKey] = (otp, expiry);

            return otp;
        }

        public bool VerifyOTP(string userKey, string inputOtp)
        {
            if (!_otpStorage.ContainsKey(userKey))
                return false;

            var (storedOtp, expiry) = _otpStorage[userKey];

            if (DateTime.Now > expiry)
            {
                _otpStorage.Remove(userKey);
                return false;
            }

            if (inputOtp == storedOtp)
            {
                _otpStorage.Remove(userKey);
                return true;
            }

            return false;
        }

        public void SendEmail(string toEmail, string otp)
        {
            string fromEmail = "khoavaden@gmail.com";
            string fromPassword = "wjyv rrqu devo ngqd";

            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail);
                    mail.To.Add(toEmail);
                    mail.Subject = "Your OTP Code";
                    mail.Body = $"Your OTP code is: {otp}";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
            catch (System.Exception e)
            {

                Console.WriteLine("Lỗi gửi mail", e.Message);
            }
        }

        public async Task SendEmailToUser(string toEmail, string subject, string body, byte[]? hoaDonPDF = null)
        {

            string fromEmail = "khoavaden@gmail.com";
            string fromPassword = "rufv cfqw wsay weim";

            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail);
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    if (hoaDonPDF != null)
                    {
                        using (MemoryStream ms = new MemoryStream(hoaDonPDF))
                        {
                            Attachment attachment = new Attachment(ms, "HoaDon.pdf", "application/pdf");
                            mail.Attachments.Add(attachment);

                            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                            {
                                smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
                                smtp.EnableSsl = true;
                                try
                                {
                                    await smtp.SendMailAsync(mail);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("==============================");
                                    Console.WriteLine("Lỗi gửi mail với đính kèm", ex.Message);
                                    Console.WriteLine("==============================");
                                }
                            }
                        }
                    }
                    else
                    {
                      
                        using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                        {
                            smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
                            smtp.EnableSsl = true;
                            await smtp.SendMailAsync(mail);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {

                Console.WriteLine("Lỗi gửi mail", e.Message);
            }

        }

    }
}