using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ClrMd.Explorer.GeekOut
{
    public class Dgml
    {
        public List<DgmlNode> Nodes { get; }
        public List<DgmlNodeLink> Links { get; }

        public Dgml()
        {
            Nodes = new List<DgmlNode>();
            Links = new List<DgmlNodeLink>();
        }

        public Dgml AddNode(string id, string label)
        {
            if (!Nodes.Any(node => node.Id == id))
            {
                Nodes.Add(new DgmlNode
                {
                    Id = id,
                    Label = label
                });
            }

            return this;
        }

        public Dgml AddLink(string source, string target)
        {
            Links.Add(new DgmlNodeLink
            {
                Source = source,
                Target = target
            });

            return this;
        }

        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("DirectedGraph", "http://schemas.microsoft.com/vs/2009/dgml");

            writer.WriteStartElement("Nodes");
            foreach (var dgmlNode in Nodes)
            {
                writer.WriteStartElement("Node");
                writer.WriteAttributeString("Id", dgmlNode.Id);
                writer.WriteAttributeString("Label", dgmlNode.Label);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Links");
            foreach (var dgmlNodeLink in Links)
            {
                writer.WriteStartElement("Link");
                writer.WriteAttributeString("Source", dgmlNodeLink.Source);
                writer.WriteAttributeString("Target", dgmlNodeLink.Target);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Properties");
                writer.WriteStartElement("Property");
                writer.WriteAttributeString("Id", "Label");
                writer.WriteAttributeString("Label", "Label");
                writer.WriteAttributeString("Description", "Displayable label of an Annotatable object");
                writer.WriteAttributeString("DataType", "System.String");
                writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }
}