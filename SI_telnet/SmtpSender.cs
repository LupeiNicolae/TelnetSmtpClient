using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SI_telnet
{
    public class SmtpSender<T> where T : AuthenticationCredentials
    {
        private readonly T _credentials;
        private TcpClient Client { get; set; }
        private NetworkStream Ns { get; set; }

        public SmtpSender(T credentials)
        {
            _credentials = credentials;
            Client = new TcpClient();
        }

        public SmtpResult Connect(string hostName, int port)
        {
            var googleSmtpIp = Dns.GetHostAddresses(hostName);
            Client.Connect(googleSmtpIp, port);
            Ns = Client.GetStream();
            var connectionStatus = Read(Ns);
            var response = new SmtpResult {
                IsSuccess = IsSuccess(connectionStatus),
                Message = connectionStatus
            };

            return response;
        }

        public SmtpResult SendMessage(SmtpMessage message)
        {
            var response = new SmtpResult();
            if (!Client.Connected)
            {
                response.Message = "Client not connected";
                return response;
            }

            Write(Ns, "HELO google");
            var helloResponse = Read(Ns);
            if (!IsSuccess(helloResponse))
            {
                response.Message = helloResponse;
                return response;
            }

            Write(Ns, "starttls auth login");
            var _ = Read(Ns);

            Write(Ns, "auth login");
            var authResponse = Read(Ns);
            if (!IsSuccess(authResponse))
            {
                response.Message = authResponse;
                return response;
            }

            var (userName, password) = _credentials.GetCredentials();

            Write(Ns, userName);
            var emailResponse = Read(Ns);
            if (!IsSuccess(emailResponse))
            {
                response.Message = emailResponse;
                return response;
            }

            Write(Ns, password);
            var passwordResponse = Read(Ns);
            if (!IsSuccess(passwordResponse))
            {
                response.Message = passwordResponse;
                return response;
            }

            Write(Ns, $"MAIL FROM:<{message.From}>");
            var mailFromResponse = Read(Ns);
            if (!IsSuccess(mailFromResponse))
            {
                response.Message = mailFromResponse;
                return response;
            }

            Write(Ns, $"RCPT TO:<{message.To}>");
            var rcptResponse = Read(Ns);
            if (!IsSuccess(rcptResponse))
            {
                response.Message = rcptResponse;
                return response;
            }

            Write(Ns, "DATA");
            var dataResponse = Read(Ns);
            if (!IsSuccess(dataResponse))
            {
                response.Message = dataResponse;
                return response;
            }

            Write(Ns, $"From: {message.FromName} <{message.From}>");
            Write(Ns, $"Subject:{message.Subject}");
            Write(Ns, message.Message);
            Write(Ns, ".");
            var sendMessageResponse = Read(Ns);
            if (!IsSuccess(sendMessageResponse))
            {
                response.Message = sendMessageResponse;
                return response;
            }

            Write(Ns, "QUIT");
            var quitResponse = Read(Ns);
            response.IsSuccess = IsSuccess(quitResponse);
            if (!response.IsSuccess)
            {
                response.Message = quitResponse;
            }

            return response;
        }

        private static bool IsSuccess(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                return false;
            }

            var code = response.Substring(0, 3);

            var isSuccess = int.Parse(code) < 400;
            if (!isSuccess)
            {
                Console.WriteLine("Error: " + response);
            }
            return isSuccess;
        }

        private static string Read(NetworkStream ns)
        {
            var sb = new StringBuilder();

            if (ns.CanRead)
            {
                var readBuffer = new byte[1024];

                do
                {
                    var numBytesRead = ns.Read(readBuffer, 0, readBuffer.Length);
                    sb.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, numBytesRead));

                    sb.Replace(Convert.ToChar(24), ' ');

                    sb.Replace(Convert.ToChar(255), ' ');

                    sb.Replace('?', ' ');
                }
                while (ns.DataAvailable);
            }
            return sb.ToString();
        }

        private static void Write(NetworkStream ns, string message)
        {
            var msg = Encoding.ASCII.GetBytes(message + Environment.NewLine);

            ns.Write(msg, 0, msg.Length);
        }
    }
}
