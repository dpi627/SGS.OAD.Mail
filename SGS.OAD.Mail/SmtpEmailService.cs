#nullable disable

using System.Net;
using System.Net.Mail;

namespace SGS.OAD.Mail
{
    /// <summary>
    /// 郵件建構器
    /// </summary>
    public class MailBuilder
    {
        // 郵件基本配置
        private string _smtpHost;
        private int _smtpPort = 587;
        private string _username;
        private string _password;
        private bool _enableSsl = true;

        // 郵件內容配置
        private string _fromEmail;
        private List<string> _toEmails = new List<string>();
        private List<string> _ccEmails = new List<string>();
        private List<string> _bccEmails = new List<string>();
        private string _subject;
        private string _body;
        private bool _isHtmlBody;
        private List<string> _attachmentPaths = new List<string>();

        /// <summary>
        /// 配置 SMTP 伺服器
        /// </summary>
        /// <returns>FluentEmailBuilder</returns>
        public MailBuilder UseSmtp(
            string host,
            int port = 587,
            bool enableSsl = true)
        {
            _smtpHost = host;
            _smtpPort = port;
            _enableSsl = enableSsl;
            return this;
        }

        /// <summary>
        /// 設定 SMTP 認證
        /// </summary>
        public MailBuilder WithCredentials(string username, string password)
        {
            _username = username;
            _password = password;
            return this;
        }

        /// <summary>
        /// 設定發件人
        /// </summary>
        public MailBuilder From(string email)
        {
            _fromEmail = email;
            return this;
        }

        /// <summary>
        /// 設定收件人
        /// </summary>
        public MailBuilder To(string email)
        {
            _toEmails.Add(email);
            return this;
        }

        /// <summary>
        /// 設定多個收件人
        /// </summary>
        public MailBuilder To(params string[] emails)
        {
            _toEmails.AddRange(emails);
            return this;
        }

        /// <summary>
        /// 設定副本收件人
        /// </summary>
        public MailBuilder Cc(string email)
        {
            _ccEmails.Add(email);
            return this;
        }

        /// <summary>
        /// 設定多個副本收件人
        /// </summary>
        public MailBuilder Cc(params string[] emails)
        {
            _ccEmails.AddRange(emails);
            return this;
        }

        /// <summary>
        /// 設定密送收件人
        /// </summary>
        public MailBuilder Bcc(string email)
        {
            _bccEmails.Add(email);
            return this;
        }

        /// <summary>
        /// 設定多個密送收件人
        /// </summary>
        public MailBuilder Bcc(params string[] emails)
        {
            _bccEmails.AddRange(emails);
            return this;
        }

        /// <summary>
        /// 設定郵件主題
        /// </summary>
        public MailBuilder Subject(string subject)
        {
            _subject = subject;
            return this;
        }

        /// <summary>
        /// 設定郵件內容
        /// </summary>
        public MailBuilder Body(string body, bool isHtml = false)
        {
            _body = body;
            _isHtmlBody = isHtml;
            return this;
        }

        /// <summary>
        /// 添加附件
        /// </summary>
        public MailBuilder Attach(string filePath)
        {
            _attachmentPaths.Add(filePath);
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
        /// 創建 SMTP 客戶端
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
        /// 創建郵件消息
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

    /// <summary>
    /// 郵件發送器的靜態入口
    /// </summary>
    public static class EmailSender
    {
        /// <summary>
        /// 創建郵件構建器的靜態方法
        /// </summary>
        public static MailBuilder Create() => new MailBuilder();
    }
}