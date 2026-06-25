//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;

//namespace VEMS.MathCore
//{
//    public class XmlHelper : XmlDocument
//    {
//        private string configFilePath;

//        public XmlHelper(string configFilePath)
//        {
//            this.configFilePath = configFilePath;
//            Load(configFilePath);
//        }

//        public void SaveXml()
//        {
//            Save(configFilePath);
//        }

//        public string GetNodeInnerText(string xpath)
//        {
//            XmlNode node = SelectSingleNode(xpath);
//            return node?.InnerText;
//        }

//        public void SetNodeInnerText(string xpath, string innerText)
//        {
//            XmlNode node = SelectSingleNode(xpath);
//            if (node != null)
//            {
//                node.InnerText = innerText;
//            }
//        }

//    }
//}
