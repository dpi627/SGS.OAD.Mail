﻿using SGS.OAD.Mail;

namespace Console6
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var mail = Email.Create()
                //.WithCredentials(@"APAC\brian_li", "******")
                //.From("brian_li@sgs.com") //預設 no-reply@sgs.com
                .To("brian.li@sgs.com") //"brian_li@sgs.com", "amy.hu@sgs.com"
                //.Cc("brian.li.sgs@gmail.com")
                //.Subject("測試郵件")
                //.Body("<h1>Hello World!</h1>", isHtml: true);
                //.Attach("/path/to/attachment.pdf")
                .DisableSSL();

                mail.Send();
        }
    }
}
