using System;
using System.Web;
using System.Collections;
using System.Xml;


namespace DASHServer.Service
{
    public class XMLService
    {

        public XmlDocument getXMLBase64Frames(string strFileName, double strartPosition, double endPosition)
        {
            strFileName = @strFileName;
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            // Create the root element
            XmlElement rootNode = xmlDoc.CreateElement("Scene");
            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);
            xmlDoc.AppendChild(rootNode);
            XmlText bas64Frame;
            XmlElement parentNode;
            double StreamLength;
            double frameRate;
            System.Drawing.Size size = System.Drawing.Size.Empty;

            string[] frameArray = VideoStreamService.getBase64FrameArray(strFileName, strartPosition, endPosition, out StreamLength, out frameRate, out size, "theMorningOutline.com");
            for (int i = 0; i < frameArray.Length; i++)
            {
                parentNode = xmlDoc.CreateElement("Frame");
                parentNode.SetAttribute("ID", i.ToString());
                xmlDoc.DocumentElement.PrependChild(parentNode);
                bas64Frame = xmlDoc.CreateTextNode(frameArray[i]);
                parentNode.AppendChild(bas64Frame);
            }

            return xmlDoc;
        }

        public  XmlDocument getXMLBase64ClipInfo(string strFileName)
        {

            double streamLength;
            double frameRate;
            System.Drawing.Size clipSize;
            string[] thumbnailBase64;

            thumbnailBase64 = VideoStreamService.getBase64FrameArray(strFileName, 0.0, 0.01, out streamLength, out frameRate, out clipSize, "theMorningOutline.com");

            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            // Create the root element
            XmlElement rootNode = xmlDoc.CreateElement("Clip");
            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);
            xmlDoc.AppendChild(rootNode);
            XmlText bas64Frame;
            XmlElement parentNode;
            parentNode = xmlDoc.CreateElement("Frame");
            parentNode.SetAttribute("ID", "0");
            xmlDoc.DocumentElement.PrependChild(parentNode);
            bas64Frame = xmlDoc.CreateTextNode(thumbnailBase64[0]);
            parentNode.AppendChild(bas64Frame);

            parentNode = xmlDoc.CreateElement("Stream_Length");
            // parentNode.SetAttribute("ID", i.ToString());
            xmlDoc.DocumentElement.PrependChild(parentNode);
            bas64Frame = xmlDoc.CreateTextNode(streamLength.ToString());
            parentNode.AppendChild(bas64Frame);

            parentNode = xmlDoc.CreateElement("Clip_Size");
            // parentNode.SetAttribute("ID", i.ToString());
            xmlDoc.DocumentElement.PrependChild(parentNode);
            bas64Frame = xmlDoc.CreateTextNode(clipSize.ToString());
            parentNode.AppendChild(bas64Frame);

            parentNode = xmlDoc.CreateElement("Frame_Rate");
            // parentNode.SetAttribute("ID", i.ToString());
            xmlDoc.DocumentElement.PrependChild(parentNode);
            bas64Frame = xmlDoc.CreateTextNode(frameRate.ToString());
            parentNode.AppendChild(bas64Frame);

            return xmlDoc;
        }
    }
   
}
