using SGS.OAD.Mail;
using System.Collections.Generic;

namespace Console472
{
    internal class Program
    {
        static async void Main()
        {
            Email.Create()
                .To("someone@sgs.com")
                .SendWithRetry();

            await Email.Create()
                .To("someone@sgs.com")
                .SendAsyncWithRetry();

            var list = new List<Email>
            {
                Email.Create().To("user1@sgs.com"),
                Email.Create().To("user2@sgs.com")
            };
            await Email.SendMultipleAsync(list);
            await Email.SendMultipleWithRetryAsync(list);
        }
    }
}
