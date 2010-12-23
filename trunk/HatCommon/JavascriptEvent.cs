using System;
using System.Collections.Generic;
using System.Text;

namespace Hatfield.Web.Portal
{
    public enum JavascriptEventName { onblur, onchange, onclick, ondblclick, onerror, onfocus, onkeydown, onkeypress, onkeyup, onmousedown, onmousemove, onmouseout, onmouseover, onmouseup, onresize, onselect, onunload };

    public class JavascriptEvent
    {
        private JavascriptEventName eventName;
        public JavascriptEventName EventName
        {
            get { return eventName; }
            set { eventName = value; }
        }

        private string eventCode = "";
        public string EventCode
        {
            get { return eventCode; }
            set { eventCode = value; }
        }

        public JavascriptEvent(JavascriptEventName eventName, string eventCode)
        {
            this.EventName = eventName;
            this.EventCode = eventCode;
        }

        public string getHtmlCode() {
            return EventName + "=\"" + eventCode + "\"";
        }
    }
}
