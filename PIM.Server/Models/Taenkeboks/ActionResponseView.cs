using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIM.Server.Models.Taenkeboks
{
    public class ActionResponseView
    {
        public ActionResponseView(HT9000.Taenkeboks.Action action, string response)
        {
            Action = action;
            Response = response;
        }
        public HT9000.Taenkeboks.Action Action { get; }
        public string Response { get; }
    }
}