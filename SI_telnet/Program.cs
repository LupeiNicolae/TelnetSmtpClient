using System;

namespace SI_telnet
{
    class Program
    {
        static void Main(string[] args)
        {

            var fromEmail = "";

            var credentials = new GoogleAuthenticationCredentials(fromEmail, "");
            var smtpSender = new SmtpSender<GoogleAuthenticationCredentials>(credentials);
            var connectResult = smtpSender.Connect("smtp.gmail.com", 587);
            if (connectResult.IsSuccess)
            {
                var message = new SmtpMessage {
                    Subject = "I'm sending mails!",
                    FromName = "Lupei Nicolae",
                    From = fromEmail,
                    To = "nicolae.lupei@indrivo.com",
                    Message = "Hello, this is test."
                };
                var sendResult = smtpSender.SendMessage(message);
                if (sendResult.IsSuccess)
                {
                    Console.WriteLine("Delivered");
                }
                else
                {
                    Console.WriteLine("Fail to deliver message, error: " + sendResult.Message);
                }
            }
        }
    }
}