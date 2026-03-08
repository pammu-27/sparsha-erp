using System.Net;
using System.Net.Mail;

namespace SparshaERP.Helpers
{
    public class EmailService
    {
        public void Send(string to, string subject, string body)
        {
            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(
                    "pammupramod811@gmail.com",
                    "ejaa fnrj jenz pgmy"
                ),
                EnableSsl = true,
            };

            smtp.Send("pammupramod811@gmail.com", to, subject, body);
        }
    }
}
