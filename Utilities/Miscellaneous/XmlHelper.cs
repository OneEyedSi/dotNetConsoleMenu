//
// $History: XmlHelper.cs $ 
// 
// *****************  Version 6  *****************
// User: Simonfi      Date: 27/02/12   Time: 8:55a
// Updated in $/UtilitiesClassLibrary/Utilities
// server changes to get web services and so on up to scratch with latest
// schemas and service broker start.
// 
// *****************  Version 5  *****************
// User: Brentfo      Date: 16/06/11   Time: 9:40a
// Updated in $/UtilitiesClassLibrary/Utilities
// Added simple helper method to remove the xml declaration from the start
// of a string containing xml as the declaration causes issues when
// converting varchar/nvarchar data types to xml in Sql.
// 
// *****************  Version 4  *****************
// User: Mikehu       Date: 5/05/11    Time: 15:45
// Updated in $/UtilitiesClassLibrary/Utilities
// 
// *****************  Version 3  *****************
// User: Reganp       Date: 4/05/11    Time: 4:03p
// Updated in $/UtilitiesClassLibrary/Utilities
// Provided overload of Serialize() method to allow the XML declaration to
// be omitted.
// 
// *****************  Version 2  *****************
// User: Mikehu       Date: 3/05/11    Time: 16:49
// Updated in $/UtilitiesClassLibrary/Utilities
// 
// *****************  Version 1  *****************
// User: Brentfo      Date: 28/11/08   Time: 2:11p
// Created in $/UtilitiesClassLibrary/Utilities
// Added the XmlHelper class to simplify serializing objects.
// 

using System;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Text;

namespace Utilities.Miscellaneous
{
    public static class XmlHelper
    {
        /// <summary>
        /// Update or insert an attribute value within an element.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        public static void SetAttribute(XmlDocument doc, XmlElement element, string attributeName, string attributeValue)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (attributeName == null)
                throw new ArgumentNullException("attributeName");

            XmlAttribute attribute = element.Attributes[attributeName];
            if (attribute == null)
                attribute = doc.CreateAttribute(attributeName);

            attribute.Value = attributeValue;

            element.Attributes.SetNamedItem(attribute);
        }

        /// <summary>
        /// Gets an attribute value from an XmlNode, if the attribute doesn't exist
        /// then it throws an XmlException.
        /// </summary>
        /// <param name="element">The XmlNode that contains the attribute.</param>
        /// <param name="attributeName">The name of the attribute to return.</param>
        /// <returns>The string value of the attribute.</returns>
        public static string GetAttribute(XmlNode element, string attributeName)
        {
            XmlAttribute attribute = element.Attributes[attributeName];
            if (attribute != null)
                return attribute.Value;
            throw new XmlException(string.Format("Missing attribute: {0}", attributeName));
        }

        /// <summary>
        /// Serialize an object to an Xml string.
        /// </summary>     
        /// <param name="type">The type of the object being serialized.</typeparam>
        /// <param name="obj">The object to be serialized.</param>
        /// <returns>An Xml string of the serialized object.</returns>
        public static string Serialize(Type type, object obj)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true; // Remove the <?xml version="1.0" encoding...

            // Create the writer object.
            StringWriter writer = new StringWriter();
            XmlWriter xmlWriter = XmlWriter.Create(writer, settings);

            // Declare some default namespaces to get ride of the xmlns: tags.
            XmlSerializerNamespaces nameSpaces = new XmlSerializerNamespaces();
            nameSpaces.Add(string.Empty, string.Empty);

            // Create the serializer and serialize the object.
            XmlSerializer serializer = new XmlSerializer(type);
            serializer.Serialize(xmlWriter, obj, nameSpaces);

            return writer.ToString();
        }

        /// <summary>
        /// Deserialize an object from an Xml string.
        /// </summary>
        /// <typeparam name="TObject">The type of the object being deserialized.</typeparam>
        /// <param name="xml">An Xml string of the serialized object.</param>
        /// <returns>The deserialized object.</returns>
        public static object Deserialize(Type type, string xml)
        {
            StringReader reader = new StringReader(xml);
            XmlSerializer serializer = new XmlSerializer(type);
            // Deserialize the message from the xml.
            return serializer.Deserialize(reader);
        }

        public static object Deserialize(Type type, string xml, string xmlns)
        {
            StringReader reader = new StringReader(xml);
            XmlSerializer serializer = new XmlSerializer(type, xmlns);
            // Deserialize the message from the xml.
            return serializer.Deserialize(reader);
        }

        public static string Serialize(object obj, XmlSerializerNamespaces namespaces, bool indent, bool omitXMLDeclaration)
        {
            XmlSerializer serXML = null;

            serXML = new XmlSerializer(obj.GetType());

            StringBuilder sb = new StringBuilder();

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = indent;
            xws.OmitXmlDeclaration = omitXMLDeclaration;

            XmlWriter xw = XmlWriter.Create(sb, xws);

            try
            {
                if (namespaces != null)
                {
                    serXML.Serialize(xw, obj, namespaces);
                }
                else
                {
                    serXML.Serialize(xw, obj);
                }
            }
            catch (Exception ex)
            {
                sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                Exception e2 = ex.InnerException;
                while (e2 != null)
                {
                    sb.AppendLine(e2.Message);
                    e2 = e2.InnerException;
                }

                throw new Exception(sb.ToString(), ex);
            }

            xw.Close();

            return sb.ToString();
        }
        public static string Serialize(object obj, XmlSerializerNamespaces namespaces, bool indent)
        {
            return XmlHelper.Serialize(obj, namespaces, indent, false);
        }
        public static string Serialize(object obj, XmlSerializerNamespaces namespaces)
        {
            return Serialize(obj, namespaces, true);
        }
        public static string Serialize(object obj, bool indent)
        {
            return Serialize(obj, null, indent);
        }

        /// <summary>
        /// Use correct Serializer to Serialize the Object into XML.
        /// This will use DataContractSerilizer or XMLSerializer as appropriate.
        /// </summary>
        /// <param name="obj">Object to Serialize</param>
        /// <returns>XML version of the Object</returns>
        public static string Serialize(object obj)
        {
            return Serialize(obj, null, true);
        }

        /// <summary>
        /// Deserialize a string into its corresponding (valid) object using the correct Deserializer.
        /// This will use DataContractSerilizer or XMLSerializer as appropriate.
        /// T is <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The Type to convert the deserialized object into</typeparam>
        /// <param name="xml">The XML String got parse</param>
        /// <returns>The Deserialized object</returns>
        public static T Deserialize<T>(string xml)
        {
            Type ty = typeof(T);

            XmlSerializer serXML = null;

            serXML = new XmlSerializer(typeof(T));

            StringReader sr = new StringReader(xml);
            XmlReader xr = XmlReader.Create(sr);

            object obj;
            try
            {
                obj = serXML.Deserialize(xr);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                Exception e2 = ex.InnerException;
                while (e2 != null)
                {
                    sb.AppendLine(e2.Message);
                    e2 = e2.InnerException;
                }
                sb.AppendFormat("Current Node: {0} (try looking at the node immediately before this one)", xr.Name/*, Environment.NewLine*/);

                throw new Exception(sb.ToString(), ex);
            }

            xr.Close();

            return (T)obj;
        }

        public static object Deserialize(string xml, Type type)
        {
            XmlSerializer xmlSerializer = null;

            xmlSerializer = new XmlSerializer(type);

            StringReader stringReader = new StringReader(xml);
            XmlReader xmlReader = XmlReader.Create(stringReader);

            object obj;
            try
            {
                obj = xmlSerializer.Deserialize(xmlReader);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                Exception e2 = ex.InnerException;
                while (e2 != null)
                {
                    sb.AppendLine(e2.Message);
                    e2 = e2.InnerException;
                }
                sb.AppendFormat("Current Node: {0} (try looking at the node immediately before this one)", xmlReader.Name/*, Environment.NewLine*/);

                throw new Exception(sb.ToString(), ex);
            }

            xmlReader.Close();

            return obj;
        }

        /// <summary>
        /// Removes the &lt;?xml...?&gt; declaration from the start of
        /// a string containing xml data.
        /// </summary>
        /// <param name="xml">The xml string to have it's xml declaration removed.</param>
        /// <returns>The original string minus the first xml declaration.</returns>
        public static string RemoveXmlDeclaration(string xml)
        {
            if (xml == null)
                return xml;

            int startIndex = xml.IndexOf("<?xml");
            // Xml rules state that the xml declaration must start at character 0.
            if (startIndex == 0)
            {
                int endIndex = xml.IndexOf("?>", startIndex);
                if (endIndex > startIndex && endIndex < xml.Length + 2)
                    return xml.Substring(endIndex + 2);
            }

            return xml;
        }
    }
}