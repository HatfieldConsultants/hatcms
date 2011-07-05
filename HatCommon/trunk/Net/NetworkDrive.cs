using System;
using System.Collections.Generic;
using System.Management;
using System.Text;
using System.Runtime.InteropServices;
using Hatfield.Web.Portal.Net;

namespace Hatfield.Web.Portal.Net
{
    public class NetworkDrive
    {
        #region API
        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2A(ref structNetResource pstNetRes, string psPassword, string psUsername, int piFlags);
        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2A(string psName, int piFlags, int pfForce);
        [DllImport("mpr.dll")]
        private static extern int WNetConnectionDialog(int phWnd, int piType);
        [DllImport("mpr.dll")]
        private static extern int WNetDisconnectDialog(int phWnd, int piType);
        [DllImport("mpr.dll")]
        private static extern int WNetRestoreConnectionW(int phWnd, string psLocalDrive);

        [StructLayout(LayoutKind.Sequential)]
        private struct structNetResource
        {
            public int iScope;
            public int iType;
            public int iDisplayType;
            public int iUsage;
            public string sLocalName;
            public string sRemoteName;
            public string sComment;
            public string sProvider;
        }

        private const int RESOURCETYPE_DISK = 0x1;

        //Standard	
        private const int CONNECT_INTERACTIVE = 0x00000008;
        private const int CONNECT_PROMPT = 0x00000010;
        private const int CONNECT_UPDATE_PROFILE = 0x00000001;
        //IE4+
        private const int CONNECT_REDIRECT = 0x00000080;
        //NT5 only
        private const int CONNECT_COMMANDLINE = 0x00000800;
        private const int CONNECT_CMD_SAVECRED = 0x00001000;

        #endregion

        #region Propertys and options
        /// <summary>
        /// Option to save credentials are reconnection...
        /// </summary>
        private bool lf_SaveCredentials = false;


        private bool lf_Persistent = false; // do not reconnect drive after logoff/reboot

        /// <summary>
        /// Option to force connection if drive is already mapped...
        /// or force disconnection if network path is not responding...
        /// </summary>
        private bool lf_Force = true;


        /// <summary>
        /// Option to prompt for user credintals when mapping a drive
        /// </summary>
        private bool ls_PromptForCredentials = false;


        private string ls_Drive = "s:";
        /// <summary>
        /// Drive to be used in mapping / unmapping...
        /// </summary>
        public string LocalDrive
        {
            get { return (ls_Drive); }
            set
            {
                if (value.Length >= 1)
                {
                    ls_Drive = value.Substring(0, 1) + ":";
                }
                else
                {
                    ls_Drive = "";
                }
            }
        }
        private string ls_ShareName = "\\\\Computer\\C$";
        /// <summary>
        /// Share address to map drive to.
        /// </summary>
        public string ShareName
        {
            get { return (ls_ShareName); }
            set { ls_ShareName = value; }
        }
        #endregion

        #region Function mapping

        /// <summary>
        /// returns String.Empty if no unused drive letters are available.
        /// </summary>
        /// <returns></returns>
        private static string getFirstUnUsedDriveLetter()
        {
            string[] choices = new string[] { "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            string[] existingDrives = getLocalDriveLeters();
            foreach (string choice in choices)
            {
                if (Array.IndexOf(existingDrives, choice + ":") == -1)
                    return choice;
            }
            return String.Empty;
        }

        private static string[] getLocalDriveLeters()
        {
            //get drive collection 
            System.Collections.ArrayList ret = new System.Collections.ArrayList();

            // source: http://www.codeproject.com/KB/cs/my_explorer.aspx
            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * From Win32_LogicalDisk ");
            ManagementObjectCollection queryCollection = query.Get();
            foreach (ManagementObject mo in queryCollection)
            {
                string driveName = mo["name"].ToString();
                ret.Add(driveName);
            }

            return (string[])ret.ToArray(typeof(string));
        }

        /// <summary>
        /// Map network drive (using supplied Username and Password)
        /// </summary>
        public IntPtr MapDrive(string UserDomain, string Username, string Password, string shareName)
        {
            string driveLetter = getFirstUnUsedDriveLetter();
            if (driveLetter == String.Empty)
                throw new Exception("No drive letter.");

            this.LocalDrive = driveLetter;
            this.ShareName = shareName;

            IntPtr impersonationHandle = UserImpersonation.StartImpersonating(UserDomain, Username, Password);
            zMapDrive(getUsername(UserDomain, Username), Password);
            return impersonationHandle;
        }

        /// <summary>
        /// Unmap network drive
        /// </summary>
        public void UnMapDrive(IntPtr impersonationHandle)
        {
            try
            {
                zUnMapDrive(this.lf_Force);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot disconnect map drive: " + ex.Message);
            }
            try
            {
                UserImpersonation.StopImpersonating(impersonationHandle);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot stop impersonating: " + ex.Message);
            }
        }

        private string getUsername(string UserDomain, string Username)
        {
            return string.Join(@"\", new string[] { UserDomain, Username });
        }

        #endregion

        #region Core functions

        // Map network drive
        private void zMapDrive(string psUsername, string psPassword)
        {
            //create struct data
            structNetResource stNetRes = new structNetResource();
            stNetRes.iScope = 2;
            stNetRes.iType = RESOURCETYPE_DISK;
            stNetRes.iDisplayType = 3;
            stNetRes.iUsage = 1;
            stNetRes.sRemoteName = ls_ShareName;
            stNetRes.sLocalName = ls_Drive;
            //prepare params
            int iFlags = 0;
            if (lf_SaveCredentials) { iFlags += CONNECT_CMD_SAVECRED; }
            if (lf_Persistent) { iFlags += CONNECT_UPDATE_PROFILE; }
            if (ls_PromptForCredentials) { iFlags += CONNECT_INTERACTIVE + CONNECT_PROMPT; }
            if (psUsername == "") { psUsername = null; }
            if (psPassword == "") { psPassword = null; }
            //if force, unmap ready for new connection
            if (lf_Force) { try { zUnMapDrive(true); } catch { } }
            //call and return
            int i = WNetAddConnection2A(ref stNetRes, psPassword, psUsername, iFlags);
            if (i > 0) { throw new System.ComponentModel.Win32Exception(i); }
        }


        // Unmap network drive	
        private void zUnMapDrive(bool pfForce)
        {
            //call unmap and return
            int iFlags = 0;
            if (lf_Persistent) { iFlags += CONNECT_UPDATE_PROFILE; }
            int i = WNetCancelConnection2A(ls_Drive, iFlags, Convert.ToInt32(pfForce));
            if (i != 0) i = WNetCancelConnection2A(ls_ShareName, iFlags, Convert.ToInt32(pfForce));  //disconnect if localname was null
            if (i > 0) { throw new System.ComponentModel.Win32Exception(i); }
        }


        // Check / Restore a network drive
        private void zRestoreDrive()
        {
            //call restore and return
            int i = WNetRestoreConnectionW(0, null);
            if (i > 0) { throw new System.ComponentModel.Win32Exception(i); }
        }

        #endregion
    }
}
