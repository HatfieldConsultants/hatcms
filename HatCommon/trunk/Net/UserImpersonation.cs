using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Hatfield.Web.Portal.Net
{
    public class UserImpersonation
    {
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int LOGON32_LOGON_BATCH = 4;
        private const int LOGON32_LOGON_SERVICE = 5;
        private const int LOGON32_LOGON_UNLOCK = 7;
        private const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        private const int LOGON32_PROVIDER_DEFAULT = 0;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken
            );
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int ImpersonateLoggedOnUser(
            IntPtr hToken
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RevertToSelf();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int CloseHandle(IntPtr hObject);

        /// <summary>
        /// returns IntPtr.Zero if not successful
        /// </summary>
        /// <param name="UserDomain"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static IntPtr StartImpersonating(string UserDomain, string Username, string Password)
        {
            try
            {
                IntPtr lnToken;
                int TResult = LogonUser(Username, UserDomain, Password, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT, out lnToken);
                if (TResult > 0)
                {
                    ImpersonateLoggedOnUser(lnToken);
                    return lnToken;
                }
            }
            catch { }
            return IntPtr.Zero;
        }

        public static void StopImpersonating(IntPtr token)
        {
            try
            {
                if (token != IntPtr.Zero)
                {
                    RevertToSelf();
                    CloseHandle(token);
                }
            }
            catch
            { }
        }
    }
}
