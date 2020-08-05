using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PowershellHelpToMd
{
    public static class XmlParser
    {
        public static List<CmdletData> ParseCmdLets(string filePath)
        {
            List<CmdletData> ret = new List<CmdletData>();
            XmlReaderSettings sett = new XmlReaderSettings();


            CmdletData currentCmdLet = null;
            CmdletParameterSetData currentPSet = null;
            CmdletParameterData currentPm = null;

            using (var st = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (XmlReader rdr = XmlReader.Create(st, sett))
            {
                while (rdr.Read())
                {
                    switch (rdr.NodeType)
                    {
                        case XmlNodeType.XmlDeclaration:
                            break;
                        case XmlNodeType.Comment:
                            var comment = rdr.Value;
                            if (string.IsNullOrEmpty(comment))
                                break;
                            comment = comment.Trim();
                            if (comment.StartsWith("Parameter set:", StringComparison.InvariantCultureIgnoreCase))
                            {
                                comment = comment.Substring(14);
                                if (currentPSet != null && currentCmdLet != null)
                                    currentCmdLet.ParameterSets.Add(currentPSet);

                                if (currentPm != null && !string.IsNullOrEmpty(currentPm.Name))
                                {
                                    if (!currentPSet.Parameters.Contains(currentPm))
                                        currentPSet.Parameters.Add(currentPm);
                                }

                                currentPSet = new CmdletParameterSetData();
                                currentPSet.Name = comment;
                            }
                            break;
                        case XmlNodeType.Element:
                            switch (rdr.NamespaceURI)
                            {
                                case "http://schemas.microsoft.com/maml/dev/command/2004/10":
                                    switch (rdr.LocalName)
                                    {
                                        case "command":
                                            if (currentCmdLet != null)
                                            {
                                                ret.Add(currentCmdLet);
                                                if (currentPSet != null && !string.IsNullOrEmpty(currentPSet.Name))
                                                {
                                                    if (!currentCmdLet.ParameterSets.Contains(currentPSet))
                                                        currentCmdLet.ParameterSets.Add(currentPSet);

                                                    if (currentPm != null && !string.IsNullOrEmpty(currentPm.Name))
                                                    {
                                                        if (!currentPSet.Parameters.Contains(currentPm))
                                                            currentPSet.Parameters.Add(currentPm);
                                                    }
                                                }
                                            }
                                            currentCmdLet = new CmdletData();
                                            break;
                                        case "name":
                                            if (currentCmdLet != null)
                                                currentCmdLet.Name = rdr.ReadInnerXml();
                                            break;
                                        case "parameter":
                                            if (currentPm != null && !string.IsNullOrEmpty(currentPm.Name))
                                            {
                                                if (!currentPSet.Parameters.Contains(currentPm))
                                                    currentPSet.Parameters.Add(currentPm);
                                            }

                                            currentPm = ParseParameter(XElement.Parse(rdr.ReadOuterXml()));
                                            if (currentPSet != null)
                                                currentPSet.Parameters.Add(currentPm);
                                            break;
                                    }
                                    break;
                                case "http://schemas.microsoft.com/maml/2004/10":
                                    switch (rdr.LocalName)
                                    {
                                        case "description":
                                            if (currentCmdLet != null)
                                                currentCmdLet.Description = ConvertDesc(XElement.Parse(rdr.ReadOuterXml()));
                                            break;
                                    }
                                    break;

                            }
                            break;
                    }
                }
            }

            if (currentCmdLet != null && !string.IsNullOrEmpty(currentCmdLet.Name))
            {
                if (!ret.Contains(currentCmdLet))
                    ret.Add(currentCmdLet);
                if (currentPSet != null && !string.IsNullOrEmpty(currentPSet.Name))
                {
                    if (!currentCmdLet.ParameterSets.Contains(currentPSet))
                        currentCmdLet.ParameterSets.Add(currentPSet);
                }
            }

            return ret;
        }

        private static CmdletParameterData ParseParameter(XElement parent)
        {
            CmdletParameterData ret = new CmdletParameterData();
            foreach (var elm in parent.Nodes())
            {
                if (elm is XElement)
                {
                    var xelm = elm as XElement;
                    switch(xelm.Name.LocalName)
                    {
                        case "name":
                            if (xelm.Name.NamespaceName.Equals("http://schemas.microsoft.com/maml/2004/10"))
                                ret.Name = xelm.Value;
                            break;
                        case "description":
                            if (xelm.Name.NamespaceName.Equals("http://schemas.microsoft.com/maml/2004/10"))
                                ret.Description = ConvertDesc(xelm);
                            break;
                    }
                }
            }

            return ret;
        }

        private static string ConvertDesc(XElement parent)
        {
            StringBuilder blr = new StringBuilder();

            return ConvertDesc(parent, blr);
        }

        private static string ConvertDesc(XElement parent, StringBuilder blr)
        {
            foreach (var elm in parent.Nodes())
            {
                if (elm is XElement)
                {
                    if ((elm as XElement).Name.LocalName.Equals("para"))
                    {
                        if (blr.Length > 0)
                        {
                            blr.AppendLine();
                            blr.AppendLine();
                        }

                        blr.Append(ConvertDesc(elm as XElement, blr));
                    }
                }
                if (elm is XText)
                {
                    return (elm as XText).Value;
                }
            }

            return blr.ToString();
        }
    }
}
