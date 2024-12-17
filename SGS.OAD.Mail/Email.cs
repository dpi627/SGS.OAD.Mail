#nullable disable

using System.Net;
using System.Net.Mail;

namespace SGS.OAD.Mail
{
    /// <summary>
    /// 郵件建構器
    /// </summary>
    public class Email
    {
        // SMTP 設定
        private string _smtpHost = "smtp-apac.sgs.net";
        private int _smtpPort = 25;
        private string _username;
        private string _password;
        private bool _enableSsl = true;

        // 郵件相關設定
        private string _fromEmail = "no-reply@sgs.com";
        private List<string> _toEmails = new List<string>();
        private List<string> _ccEmails = new List<string>();
        private List<string> _bccEmails = new List<string>();
        private string _subject;
        private string _body;
        private bool _isHtmlBody;
        private List<string> _attachmentPaths = new List<string>();

        /// <summary>
        /// 創建郵件構建器的靜態方法
        /// </summary>
        public static Email Create() => new Email();


        /// <summary>
        /// 設定 SMTP
        /// </summary>
        /// <returns>FluentEmailBuilder</returns>
        public Email UseSmtp(
            string host,
            int port,
            bool enableSsl = false)
        {
            _smtpHost = host;
            _smtpPort = port;
            _enableSsl = enableSsl;
            return this;
        }

        /// <summary>
        /// 設定 SMTP 認證
        /// </summary>
        public Email WithCredentials(string username, string password)
        {
            _username = username;
            _password = password;
            return this;
        }

        /// <summary>
        /// 設定發件人
        /// </summary>
        public Email From(string email)
        {
            _fromEmail = email;
            return this;
        }

        /// <summary>
        /// 設定收件人
        /// </summary>
        public Email To(string email)
        {
            _toEmails.Add(email);
            return this;
        }

        /// <summary>
        /// 設定多個收件人
        /// </summary>
        public Email To(params string[] emails)
        {
            _toEmails.AddRange(emails);
            return this;
        }

        /// <summary>
        /// 設定副本收件人
        /// </summary>
        public Email Cc(string email)
        {
            _ccEmails.Add(email);
            return this;
        }

        /// <summary>
        /// 設定多個副本收件人
        /// </summary>
        public Email Cc(params string[] emails)
        {
            _ccEmails.AddRange(emails);
            return this;
        }

        /// <summary>
        /// 設定密送收件人
        /// </summary>
        public Email Bcc(string email)
        {
            _bccEmails.Add(email);
            return this;
        }

        /// <summary>
        /// 設定多個密送收件人
        /// </summary>
        public Email Bcc(params string[] emails)
        {
            _bccEmails.AddRange(emails);
            return this;
        }

        /// <summary>
        /// 設定郵件主題
        /// </summary>
        public Email Subject(string subject)
        {
            _subject = subject;
            return this;
        }

        /// <summary>
        /// 設定郵件內容
        /// </summary>
        public Email Body(string body, bool isHtml = false)
        {
            _body = body;
            _isHtmlBody = isHtml;
            return this;
        }

        /// <summary>
        /// 加入附件
        /// </summary>
        public Email Attach(string filePath)
        {
            _attachmentPaths.Add(filePath);
            return this;
        }

        /// <summary>
        /// 加入多個附件
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        public Email Attach(params string[] filePaths)
        {
            _attachmentPaths.AddRange(filePaths);
            return this;
        }

        /// <summary>
        /// 建立並傳送郵件（同步）
        /// </summary>
        public void Send()
        {
            ValidateConfiguration();

            using (var smtpClient = CreateSmtpClient())
            {
                var mailMessage = CreateMailMessage();
                smtpClient.Send(mailMessage);
            }
        }

        /// <summary>
        /// 建立並傳送郵件（非同步）
        /// </summary>
        public async Task SendAsync()
        {
            ValidateConfiguration();

            using (var smtpClient = CreateSmtpClient())
            {
                var mailMessage = CreateMailMessage();
                await Task.Run(() => smtpClient.Send(mailMessage));
            }
        }

        /// <summary>
        /// 驗證郵件配置
        /// </summary>
        private void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_smtpHost))
                throw new InvalidOperationException("未設定 SMTP 伺服器");

            if (string.IsNullOrEmpty(_fromEmail))
                throw new InvalidOperationException("未設定發件人");

            if (_toEmails.Count == 0)
                throw new InvalidOperationException("未設定收件人");

            if (string.IsNullOrEmpty(_subject))
                throw new InvalidOperationException("未設定郵件主題");

            if (string.IsNullOrEmpty(_body))
                throw new InvalidOperationException("未設定郵件內容");
        }

        /// <summary>
        /// 建立 SMTP Client
        /// </summary>
        private SmtpClient CreateSmtpClient()
        {
            var smtpClient = new SmtpClient(_smtpHost)
            {
                Port = _smtpPort,
                EnableSsl = _enableSsl
            };

            if (!string.IsNullOrEmpty(_username))
            {
                smtpClient.Credentials = new NetworkCredential(_username, _password);
            }

            return smtpClient;
        }

        /// <summary>
        /// 建立郵件本體
        /// </summary>
        private MailMessage CreateMailMessage()
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = _subject,
                Body = _body,
                IsBodyHtml = _isHtmlBody
            };

            // 添加收件人
            _toEmails.ForEach(email => mailMessage.To.Add(email));
            _ccEmails.ForEach(email => mailMessage.CC.Add(email));
            _bccEmails.ForEach(email => mailMessage.Bcc.Add(email));

            // 添加附件
            _attachmentPaths.ForEach(path =>
                mailMessage.Attachments.Add(new Attachment(path)));

            return mailMessage;
        }
    }
}