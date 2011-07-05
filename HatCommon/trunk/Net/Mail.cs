using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Net.Mail;

namespace Hatfield.Web.Portal.Net
{
    /// <summary>
    /// provides methods to send email via smtp direct to mail server
    /// source: http://www.eggheadcafe.com/articles/20030316.asp
    /// </summary>
    public class SmtpDirect
    {
        /// <summary>
        /// Get / Set the name of the SMTP mail server
        /// </summary>
        public static string SmtpServerHostName;
        public static int SmtpServerPortNumber = 25;
        public static NetworkCredential SmtpServerCredentials = null;

        private enum SMTPResponse : int
        {
            CONNECT_SUCCESS = 220,
            GENERIC_SUCCESS = 250,
            AUTHSTEP_SUCCESS = 334,
            AUTH_SUCCESS = 235,
            DATA_SUCCESS = 354,
            QUIT_SUCCESS = 221
        }

        public static bool Send(System.Net.Mail.MailMessage message)
        {
            IPHostEntry IPhst = Dns.GetHostEntry(SmtpServerHostName);
            IPEndPoint endPt = new IPEndPoint(IPhst.AddressList[0], SmtpServerPortNumber);
            Socket s = new Socket(endPt.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Connect(endPt);

                if (!Check_Response(s, SMTPResponse.CONNECT_SUCCESS))
                {
                    s.Close();
                    return false;
                }

                Senddata(s, string.Format("HELO {0}\r\n", Dns.GetHostName()));
                if (!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
                {
                    s.Close();
                    return false;
                }

                if (SmtpServerCredentials != null)
                {
                    // -- walkthough here: http://www.technoids.org/saslmech.html 
                    Senddata(s, "AUTH LOGIN\r\n");
                    // 334 VXNlcm5hbWU6 (S: 334 Username:)
                    if (!Check_Response(s, SMTPResponse.AUTHSTEP_SUCCESS))
                    {
                        s.Close();
                        return false;
                    }
                    // -- send username
                    string un_encoded = StringUtils.Base64Encode(SmtpServerCredentials.UserName);
                    Senddata(s, un_encoded + "\r\n");
                    if (!Check_Response(s, SMTPResponse.AUTHSTEP_SUCCESS))
                    {
                        s.Close();
                        return false;
                    }

                    string pw_encoded = StringUtils.Base64Encode(SmtpServerCredentials.Password);
                    Senddata(s, pw_encoded + "\r\n");
                    if (!Check_Response(s, SMTPResponse.AUTH_SUCCESS))
                    {
                        s.Close();
                        return false;
                    }


                } // if need authentication

                Senddata(s, string.Format("MAIL From: {0}\r\n", message.From.Address));
                if (!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
                {

                    s.Close();
                    return false;
                }


                foreach (MailAddress To in message.To)
                {
                    Senddata(s, string.Format("RCPT TO: {0}\r\n", To.Address));
                    if (!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
                    {
                        s.Close();
                        return false;
                    }
                }

                if (message.CC.Count > 0)
                {

                    foreach (MailAddress To in message.CC)
                    {
                        Senddata(s, string.Format("RCPT TO: {0}\r\n", To.Address));
                        if (!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
                        {
                            s.Close();
                            return false;
                        }
                    }
                }

                if (message.Bcc.Count > 0)
                {

                    foreach (MailAddress To in message.Bcc)
                    {
                        Senddata(s, string.Format("RCPT TO: {0}\r\n", To.Address));
                        if (!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
                        {
                            s.Close();
                            return false;
                        }
                    }
                }

                Senddata(s, ("DATA\r\n"));
                if (!Check_Response(s, SMTPResponse.DATA_SUCCESS))
                {
                    s.Close();
                    return false;
                }

                StringBuilder Header = new StringBuilder();
                // From: "Ruri Iskandar" <ruri@hatfieldgroup.com>
                string displayName = message.From.DisplayName;
                if (displayName.Trim() == "")
                    displayName = message.From.Address;
                Header.Append("From: \"" + displayName + "\" <" + message.From.Address + ">\r\n");
                // Tos= message.To.Split(new char[] {';'});
                Header.Append("To: ");
                for (int i = 0; i < message.To.Count; i++)
                {
                    Header.Append(i > 0 ? "," : "");
                    Header.Append(message.To[i]);
                }
                Header.Append("\r\n");
                if (message.CC.Count > 0)
                {
                    // Tos= message.CC..Split(new char[] {';'});
                    Header.Append("Cc: ");
                    for (int i = 0; i < message.CC.Count; i++)
                    {
                        Header.Append(i > 0 ? "," : "");
                        Header.Append(message.CC[i]);
                    }
                    Header.Append("\r\n");
                }
                Header.Append("Date: ");
                // should be: "Date: Mon, 14 Apr 2008 00:51:22 -0700 (PDT)"
                // Mon, 14 4 8 14:52:34 +7
                string dt = DateTime.Now.ToString("ddd, d MMM yyyy H:m:s zzz");
                Header.Append(dt);
                Header.Append("\r\n");
                Header.Append("Subject: " + message.Subject + "\r\n");
                Header.Append("X-Mailer: Hatfield_WebSMTPDirect v1 (www.hatfieldgroup.com)\r\n");
                if (message.IsBodyHtml)
                    Header.Append("Content-Type: text/html\r\n");
                else
                    Header.Append("Content-Type: text/plain\r\n");

                string MsgBody = getMessageBody(message);
                if (!MsgBody.EndsWith("\r\n"))
                    MsgBody += "\r\n";
                if (message.Attachments.Count > 0)
                {
                    Header.Append("MIME-Version: 1.0\r\n");
                    Header.Append("Content-Type: multipart/mixed; boundary=unique-boundary-1\r\n");
                    Header.Append("\r\n");
                    Header.Append("This is a multi-part message in MIME format.\r\n");
                    StringBuilder sb = new StringBuilder();
                    sb.Append("--unique-boundary-1\r\n");
                    if (message.IsBodyHtml)
                        sb.Append("Content-Type: text/html\r\n");
                    else
                        sb.Append("Content-Type: text/plain\r\n");

                    sb.Append("Content-Transfer-Encoding: 7Bit\r\n");
                    sb.Append("\r\n");
                    sb.Append(MsgBody + "\r\n");
                    sb.Append("\r\n");

                    foreach (Attachment a in message.Attachments)
                    {
                        byte[] binaryData;
                        if (a != null)
                        {
                            FileInfo f = new FileInfo(a.Name);
                            sb.Append("--unique-boundary-1\r\n");
                            sb.Append("Content-Type: application/octet-stream; file=" + f.Name + "\r\n");
                            sb.Append("Content-Transfer-Encoding: base64\r\n");
                            sb.Append("Content-Disposition: attachment; filename=" + f.Name + "\r\n");
                            sb.Append("\r\n");
                            FileStream fs = f.OpenRead();
                            binaryData = new Byte[fs.Length];
                            long bytesRead = fs.Read(binaryData, 0, (int)fs.Length);
                            fs.Close();
                            string base64String = System.Convert.ToBase64String(binaryData, 0, binaryData.Length);

                            for (int i = 0; i < base64String.Length; )
                            {
                                int nextchunk = 100;
                                if (base64String.Length - (i + nextchunk) < 0)
                                    nextchunk = base64String.Length - i;
                                sb.Append(base64String.Substring(i, nextchunk));
                                sb.Append("\r\n");
                                i += nextchunk;
                            }
                            sb.Append("\r\n");
                        }
                    } // foreach
                    MsgBody = sb.ToString();
                } // if there are attachments


                Header.Append("\r\n");
                Header.Append(MsgBody);
                Header.Append(".\r\n");
                // Header.Append( "\r\n" );
                // Header.Append( "\r\n" );
                Senddata(s, Header.ToString());
                if (!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
                {
                    s.Close();
                    return false;
                }

                Senddata(s, "QUIT\r\n");
                Check_Response(s, SMTPResponse.QUIT_SUCCESS);
                return true;
            }

            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                s.Close();
            }
            return false;

        }

        private static string getMessageBody(MailMessage message)
        {
            if (message.IsBodyHtml)
            {
                StringBuilder body = new StringBuilder();
                body.Append("<html>");
                body.Append("<head><META http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\">");
                body.Append("<title>" + message.Subject + "</title>");
                body.Append("</head>");
                body.Append("<body>");
                body.Append(message.Body);
                body.Append("</body>");
                body.Append("</html>");
                return body.ToString();

            }
            else
            {
                return message.Body;
            }
        }

        private static void Senddata(Socket s, string msg)
        {
            byte[] _msg = Encoding.ASCII.GetBytes(msg);
            s.Send(_msg, 0, _msg.Length, SocketFlags.None);
        }

        private static bool Check_Response(Socket s, SMTPResponse response_expected)
        {
            string sResponse;
            int response;
            byte[] bytes = new byte[1024];
            while (s.Available == 0)
            {
                System.Threading.Thread.Sleep(100);
            }

            s.Receive(bytes, 0, s.Available, SocketFlags.None);
            sResponse = Encoding.ASCII.GetString(bytes);
            response = Convert.ToInt32(sResponse.Substring(0, 3));
            if (response != (int)response_expected)
                return false;
            return true;
        }
    }
}
