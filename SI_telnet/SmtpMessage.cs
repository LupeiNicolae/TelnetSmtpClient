﻿namespace SI_telnet
{
    public class SmtpMessage
    {
        public string Subject { get; set; }
        public string FromName { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
    }
}