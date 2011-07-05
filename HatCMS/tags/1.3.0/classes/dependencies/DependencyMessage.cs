using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS
{
    /// <summary>
    /// A message set by a dependency
    /// </summary>
    public class CmsDependencyMessage
    {
        public enum MessageLevel { Status, Warning, Error };

        public MessageLevel Level = MessageLevel.Status;
        public string Message = "";
        public CmsDependencyMessage(MessageLevel level, string message)
        {
            Level = level;
            Message = message;
        }

        public static CmsDependencyMessage Status(string message)
        {
            return new CmsDependencyMessage(MessageLevel.Status, message);
        }

        public static CmsDependencyMessage Warning(string message)
        {
            return new CmsDependencyMessage(MessageLevel.Warning, message);
        }

        public static CmsDependencyMessage Error(string message)
        {
            return new CmsDependencyMessage(MessageLevel.Error, message);
        }

        public static CmsDependencyMessage Error(Exception ex)
        {
            return new CmsDependencyMessage(MessageLevel.Error, ex.Message);
        }

        public static HatCMS.setup.setupPage.ConfigValidationMessage[] ToConfigValidationMessages(CmsDependencyMessage[] dMsgs)
        {
            List<HatCMS.setup.setupPage.ConfigValidationMessage> ret = new List<HatCMS.setup.setupPage.ConfigValidationMessage>();

            foreach (CmsDependencyMessage dMsg in dMsgs)
            {
                bool isValid = (dMsg.Level == MessageLevel.Status);
                HatCMS.setup.setupPage.ConfigValidationMessage cMsg = new HatCMS.setup.setupPage.ConfigValidationMessage(isValid, dMsg.Message);
                ret.Add(cMsg);
            }

            return ret.ToArray();
        }

    }
}
