using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProxyHttpOnly
{
    class XmlSettingsParsercs
    {
        public static List<string> GetBlockedWebsites()
        {
            var settingsFile = new XmlDocument();
            settingsFile.Load(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "BlackList.xml");

            XmlNodeList blockedWebsites = settingsFile.SelectNodes("/Settings/BlockedWebsites/Website");

            var returnList = new List<string>();

            for (int i = 0; i < blockedWebsites.Count; ++i)
            {
                returnList.Add(blockedWebsites[i].FirstChild.Value);
            }

            return returnList;
        }
    }
}
