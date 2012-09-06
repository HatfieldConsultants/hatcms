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
    /// A dependecy that requires that a config item must (or must not) be present.
    /// </summary>
    public class CmsConfigItemDependency : CmsDependency
    {        

        string configKey = "";
        ExistsMode _mode = ExistsMode.MustExist;

        public CmsConfigItemDependency(string ConfigKey)
        {
            configKey = ConfigKey;
        }

        public CmsConfigItemDependency(string ConfigKey, ExistsMode mode)
        {
            configKey = ConfigKey;
            _mode = mode;
        }

        public override string GetContentHash()
        {
            return configKey.Trim().ToLower();
        }


        public override CmsDependencyMessage[] ValidateDependency()
        {
            
            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
            switch (_mode)
            {
                case ExistsMode.MustExist:
                    if (CmsConfig.KeyExists(configKey) == false)
                    {
                        ret.Add(CmsDependencyMessage.Error("Required configuration key \"" + configKey + "\" is not set in the web.config file"));
                    }
                    break;
                case ExistsMode.MustNotExist:
                    if (CmsConfig.KeyExists(configKey) == true)
                    {
                        ret.Add(CmsDependencyMessage.Error("Configuration key \"" + configKey + "\" should NOT exist in the web.config file"));
                    }
                    break;
                default: throw new ArgumentException("Error: invalid validation mode"); break;
            }
            return ret.ToArray(); 
        }
    }  

}
