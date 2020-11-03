using System;
using System.Collections.Generic;
using System.Text;

namespace OwnLink.Model
{
    public class JsonClass
    {
        public class regJsonItem
        {
            public string status { get; set; }
            public string message { get; set; }
            public string login { get; set; }
            public string password { get; set; }
        }

        public class historyRootObject
        {
            public List<historyJsonItem> history { get; set; }
        }

        public class historyJsonItem
        {
            public string id { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public string answer { get; set; }
            public string duration { get; set; }
            public string billsec { get; set; }
            public string src { get; set; }
            public string disposition { get; set; }
        }

        public class textMessageJsonItem
        {
            public string status { get; set; }
            public string message { get; set; }
        }
    }
}
