using SGS.OAD.Mail;

namespace Console472
{
    internal class Program
    {
        static void Main()
        {
            Email.Create()
                .To("brian.li@sgs.com")
                .Send();
        }
    }
}
