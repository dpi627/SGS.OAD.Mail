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
        private string _smtpHost;
        private int _smtpPort;
        private string _username;
        private string _password;
        private bool _enableSsl;

        // 郵件相關設定
        private string _fromEmail;
        private readonly List<string> _toEmails = new();
        private readonly List<string> _ccEmails = new();
        private readonly List<string> _bccEmails = new();
        private string _subject;
        private string _body;
        private bool _isHtmlBody;
        private readonly List<string> _attachmentPaths = new();

        /// <summary>
        /// 創建郵件構建器的靜態方法
        /// </summary>
        public static Email Create() => new();

        /// <summary>
        /// 私有建構子，讀取設定檔、設定預設值
        /// </summary>
        private Email()
        {
            _smtpHost = ConfigHelper.GetValue("SMTP_HOST");
            _smtpPort = ConfigHelper.GetValue<int>("SMTP_PORT");
            _enableSsl = ConfigHelper.GetValue<bool>("SMTP_ENABLE_SSL");
            _username = ConfigHelper.GetValue("SMTP_USERNAME");
            _password = ConfigHelper.GetValue("SMTP_PASSWORD");
            _fromEmail = ConfigHelper.GetValue("MAIL_FROM");
            _subject = ConfigHelper.GetValue("MAIL_SUBJECT");
            _body = ConfigHelper.GetValue("MAIL_BODY");
        }

        public static void SendTestEmail(string testEmail)
        {
            Create().To(testEmail).Send();
        }


        /// <summary>
        /// 設定 SMTP
        /// </summary>
        public Email UseSmtp(string host, int port)
        {
            _smtpHost = host;
            _smtpPort = port;
            return this;
        }

        /// <summary>
        /// 停用 SSL
        /// </summary>
        public Email DisableSSL()
        {
            _enableSsl = false;
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
        /// 設定多個收件人
        /// </summary>
        public Email To(List<string> emails)
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
        /// 設定多個副本收件人
        /// </summary>
        /// <param name="emails"></param>
        /// <returns></returns>
        public Email Cc(List<string> emails)
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
        /// 設定多個密送收件人
        /// </summary>
        /// <param name="emails"></param>
        /// <returns></returns>
        public Email Bcc(List<string> emails)
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
        /// 加入多個附件
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        public Email Attach(List<string> filePaths)
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

            using var smtpClient = CreateSmtpClient();
            var mailMessage = CreateMailMessage();
            smtpClient.Send(mailMessage);
        }

        /// <summary>
        /// 建立並傳送郵件（非同步）
        /// SMTP Clinet 是一次性發送整封郵件給所有收件人，不需併發處理
        /// </summary>
        public async Task SendAsync()
        {
            ValidateConfiguration();

            using var smtpClient = CreateSmtpClient();
            var mailMessage = CreateMailMessage();
            await Task.Run(() => smtpClient.Send(mailMessage));
        }

        /// <summary>
        /// 建立並傳送多封郵件（非同步併發）
        /// 發送多封不同的郵件給不同的收件人，使用併發處理提高效能
        /// </summary>
        public static async Task SendMultipleAsync(List<Email> emails)
        {
            var tasks = emails.Select(email => email.SendAsync()).ToArray();
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 建立並傳送多封郵件（非同步併發），並附帶重試機制
        /// 發送多封不同的郵件給不同的收件人，使用併發處理提高效能
        /// </summary>
        /// <param name="emails">郵件清單</param>
        /// <param name="maxRetries">最大重試次數，預設3次</param>
        /// <param name="delayMilliseconds">重試間隔，預設3秒</param>
        /// <returns></returns>
        public static async Task SendMultipleWithRetryAsync(List<Email> emails, int maxRetries = 3, int delayMilliseconds = 3000)
        {
            var tasks = emails.Select(email => email.SendAsyncWithRetry(maxRetries, delayMilliseconds)).ToArray();
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 驗證資料設定
        /// </summary>
        private void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_smtpHost))
                throw new InvalidOperationException(ConfigHelper.GetValue("MSG_SMTP"));

            if (string.IsNullOrEmpty(_fromEmail))
                throw new InvalidOperationException(ConfigHelper.GetValue("MSG_FROM"));

            if (_toEmails.Count == 0)
                throw new InvalidOperationException(ConfigHelper.GetValue("MSG_TO"));

            if (string.IsNullOrEmpty(_subject))
                throw new InvalidOperationException(ConfigHelper.GetValue("MSG_SUBJECT"));

            if (string.IsNullOrEmpty(_body))
                throw new InvalidOperationException(ConfigHelper.GetValue("MSG_BODY"));
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

        /// <summary>
        /// 帶重試機制的郵件發送（同步）
        /// </summary>
        /// <param name="maxRetries">最大重試次數，預設3次</param>
        /// <param name="delayMilliseconds">重試間隔，預設3秒</param>
        public void SendWithRetry(
            int maxRetries = 3,
            int delayMilliseconds = 3000)
        {
            RetryOnFailure(() =>
            {
                ValidateConfiguration();
                using var smtpClient = CreateSmtpClient();
                var mailMessage = CreateMailMessage();
                smtpClient.Send(mailMessage);
            }, maxRetries, delayMilliseconds);
        }

        /// <summary>
        /// 帶重試機制的郵件非同步發送
        /// </summary>
        /// <param name="maxRetries">最大重試次數，預設3次</param>
        /// <param name="delayMilliseconds">重試間隔，預設3秒</param>
        public async Task SendAsyncWithRetry(
            int maxRetries = 3,
            int delayMilliseconds = 3000)
        {
            await RetryOnFailureAsync(async () =>
            {
                ValidateConfiguration();
                using var smtpClient = CreateSmtpClient();
                var mailMessage = CreateMailMessage();
                await Task.Run(() => smtpClient.Send(mailMessage));
            }, maxRetries, delayMilliseconds);
        }

        /// <summary>
        /// 失敗重試
        /// </summary>
        private static void RetryOnFailure(
            Action action,
            int maxRetries = 3,
            int delayMilliseconds = 10000)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    action();
                    return;
                }
                catch
                {
                    if (attempt == maxRetries)
                        throw;

                    Thread.Sleep(delayMilliseconds);
                }
            }
        }

        /// <summary>
        /// 失敗重試(非同步)
        /// </summary>
        private static async Task RetryOnFailureAsync(
            Func<Task> action,
            int maxRetries = 3,
            int delayMilliseconds = 10000)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await action();
                    return;
                }
                catch
                {
                    if (attempt == maxRetries)
                        throw;

                    await Task.Delay(delayMilliseconds);
                }
            }
        }
    }
}