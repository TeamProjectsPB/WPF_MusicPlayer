using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Wpf_MusicPlayer.Model;
using WMPLib;

namespace Wpf_MusicPlayer.Model
{
    public static class ConfigFile
    {
        #region Getters
        public static int GetVolume(string url)
        {
            int volume = 50;
            if (File.Exists(url))
            {
                try
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(url);
                    XmlNodeList xmlNodeList = document.GetElementsByTagName("head");
                    foreach (XmlNode xmlNode in xmlNodeList)
                    {
                        var xmlNodeList2 = xmlNode.ChildNodes;
                        foreach (XmlNode xmlNode2 in xmlNodeList2)
                        {
                            if (xmlNode2.Name.Equals("volume"))
                            {
                                int.TryParse(xmlNode2.InnerText, out volume);
                            }
                        }
                    }
                }
                catch{ }
            }
            return volume;
        }

        public static List<Tuple<string, string>> GetLibraries(string url)
        {
            List<Tuple<string, string>> tuples = new List<Tuple<string, string>>();
            //List<string> libraries = new List<string>();
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(url);

                XmlNodeList xmlNodeList = document.GetElementsByTagName("libraries");
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    var xmlNodeList2 = xmlNode.ChildNodes;
                    foreach (XmlNode xmlNode2 in xmlNodeList2)
                    {
                        if (xmlNode2.Name.Equals("library"))
                        {
                            if (xmlNode2.Attributes["name"] != null && xmlNode2.Attributes["url"] != null)
                            {
                                var name = xmlNode2.Attributes["name"].Value;
                                var libUrl = xmlNode2.Attributes["url"].Value;
                                tuples.Add(Tuple.Create(name, libUrl));
                            }
                        }
                    }
                }
            }
            catch { }
            return tuples;
        }
        public static List<string> GetPlaylists(string url)
        {
            List<string> playlists = new List<string>();
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(url);

                XmlNodeList xmlNodeList = document.GetElementsByTagName("playlists");
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    var xmlNodeList2 = xmlNode.ChildNodes;
                    foreach (XmlNode xmlNode2 in xmlNodeList2)
                    {
                        if (xmlNode2.Name.Equals("playlist"))
                        {
                            playlists.Add(xmlNode2.InnerText);
                        }
                    }
                }
            }
            catch { }
            return playlists;
        }
        #endregion

        #region Save

        public static void SaveVolume(string url, int volume)
        {
            //XDocument xDocument = XDocument.Load(fileUrl);
            //XElement root = xDocument.Element("player");
            //IEnumerable<XElement> rows = root.Descendants("head");
            if (File.Exists(url))
            {
                try
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(url);
                    
                    XmlNodeList xmlNodeList = document.GetElementsByTagName("head");
                    foreach (XmlNode xmlNode in xmlNodeList)
                    {
                        var xmlNodeList2 = xmlNode.ChildNodes;
                        bool exists = false;
                        foreach (XmlNode xmlNode2 in xmlNodeList2)
                        {
                            if (xmlNode2.Name.Equals("volume"))
                            {
                                xmlNode2.InnerText = volume.ToString();
                                exists = true;
                            }
                        }
                        if (!exists)
                        {
                            XmlNode volumeNode = document.CreateNode(XmlNodeType.Element, "volume", "");
                            volumeNode.InnerText = volume.ToString();
                            xmlNode.AppendChild(volumeNode);
                        }
                    }
                    document.Save(url);
                }
                catch { }
            }
        }
        public static void SaveNewPlaylist(string url, string name)
        {
            //XDocument xDocument = XDocument.Load(fileUrl);
            //XElement root = xDocument.Element("player");
            //IEnumerable<XElement> rows = root.Descendants("head");
            if (File.Exists(url))
            {
                try
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(url);

                    XmlNode playlists = document.GetElementsByTagName("playlists")[0];

                    XmlNode node = document.CreateNode(XmlNodeType.Element, "playlist", "");
                    node.InnerText = name;
                    playlists.AppendChild(node);
                    document.Save(url);
                }
                catch { }
            }
        }
        public static void SaveNewLibrary(string url, string name, string libUrl)
        {
            //XDocument xDocument = XDocument.Load(fileUrl);
            //XElement root = xDocument.Element("player");
            //IEnumerable<XElement> rows = root.Descendants("head");
            if (File.Exists(url))
            {
                try
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(url);

                    XmlNode playlists = document.GetElementsByTagName("libraries")[0];

                    XmlNode node = document.CreateNode(XmlNodeType.Element, "library", "");
                    {
                        XmlAttribute nameAttribute = document.CreateAttribute("", "name", "");
                        nameAttribute.Value = name;
                        node.Attributes.Append(nameAttribute);

                        XmlAttribute urlAttribute = document.CreateAttribute("", "url", "");
                        urlAttribute.Value = libUrl;
                        node.Attributes.Append(urlAttribute);
                    }

                    playlists.AppendChild(node);
                    document.Save(url);
                }
                catch { }
            }
        }
        #endregion
        #region Remove
        public static void RemoveLibrary(string fileUrl, string libName)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(fileUrl);

                XmlNodeList xmlNodeList = document.GetElementsByTagName("libraries");
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    var xmlNodeList2 = xmlNode.ChildNodes;
                    foreach (XmlNode xmlNode2 in xmlNodeList2)
                    {
                        if (xmlNode2.Name.Equals("library"))
                        {
                            if (xmlNode2.Attributes["name"].Value.Equals(libName))
                            {
                                xmlNode.RemoveChild(xmlNode2);
                            }
                        }
                    }
                }
                document.Save(fileUrl);
           }
         catch { }
        }

        public static void RemovePlaylist(string fileUrl, string playlistName)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(fileUrl);

                XmlNodeList xmlNodeList = document.GetElementsByTagName("playlists");
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    var xmlNodeList2 = xmlNode.ChildNodes;
                    foreach (XmlNode xmlNode2 in xmlNodeList2)
                    {
                        if (xmlNode2.Name.Equals("playlist"))
                        {
                            if (xmlNode2.Attributes["name"].Value.Equals(playlistName))
                            {
                                xmlNode.RemoveChild(xmlNode2);
                            }
                        }
                    }
                }
                document.Save(fileUrl);
            }
            catch { }
        }
        #endregion

        public static void CreateNewFile(string url)
        {
            XmlTextWriter writer = new XmlTextWriter(url, null);
            writer.WriteStartDocument();
            writer.WriteWhitespace("\n");
            {
                writer.WriteStartElement("player");
                {
                    writer.WriteWhitespace("\n\t");
                    writer.WriteStartElement("head");
                    {

                    }
                    writer.WriteEndElement();
                    writer.WriteWhitespace("\n\t");
                    writer.WriteStartElement("playlists");
                    {

                    }
                    writer.WriteEndElement();
                    writer.WriteWhitespace("\n\t");
                    writer.WriteStartElement("libraries");
                    {
     
                    }
                    writer.WriteEndElement();
                }
                writer.WriteWhitespace("\n");
                writer.WriteEndElement();
            }
            writer.WriteEndDocument();
            writer.Close();
        }
        private static void SaveNewFile(string url, int volume, List<string> playlists, List<Library> libraries)
        {
            XmlTextWriter writer = new XmlTextWriter(url, null);
            writer.WriteStartDocument();
            writer.WriteWhitespace("\n");
            {
                writer.WriteStartElement("player");
                {
                    writer.WriteWhitespace("\n\t");
                    writer.WriteStartElement("head");
                    {
                        writer.WriteWhitespace("\n\t\t");
                        writer.WriteAttributeString("volume", volume.ToString());
                    }
                    writer.WriteEndElement();
                    writer.WriteWhitespace("\n\t");
                    writer.WriteStartElement("playlists");
                    {
                        playlists.ForEach(x =>
                        {
                            writer.WriteWhitespace("\n\t\t");
                            writer.WriteElementString("playlist", x);
                        });
                    }
                    writer.WriteEndElement();
                    writer.WriteWhitespace("\n\t\t");
                    writer.WriteStartElement("libraries");
                    {
                        libraries.ForEach(x =>
                        {
                            writer.WriteWhitespace("\n\t\t");
                            writer.WriteAttributeString("name", x.Name);
                            writer.WriteAttributeString("url", x.Url);
                        });
                    }
                    writer.WriteEndElement();
                }
            } 
        }

        
    }
}
