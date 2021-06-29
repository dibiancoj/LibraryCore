using LibraryCore.Core.ExtensionMethods;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace LibraryCore.Core.XmlSerialization
{
    public class XMLSerializationHelper
    {

        // Add [System.Xml.Serialization.XmlRoot("Transaction")] To The Top Of The Class To Control The Root Or The Node You Are Serializing
        // Also use this for properties to serialize as attribute[System.Xml.Serialization.XmlAttribute("TheAttributeName")]

        #region Documentation On How To Remove Null Xml Output For Nullable Data Types

        // if you have a property that is a nullable int or a nullable enum...

        //the xml serializer will output <StartDateValue xmlns:d4p1="http://www.w3.org/2001/XMLSchema-instance" d4p1:nil="true" />...instead it doesn't output it

        //if you don't want to output add a method to your object like below

        ////method name needs to be "ShouldSerialize" then the property name (shady Microsoft)

        //        public bool ShouldSerializeTextBoxFilterType()
        //        {
        //            return TextBoxFilterType.HasValue;
        //        }

        //example with multiple

        // public StringFilters.StringFilterSearchType? TextBoxFilterType { get; set; }
        // public DateTime? StartDateValue { get; set; }

        //        public bool ShouldSerializeTextBoxFilterType()
        //        {
        //            return TextBoxFilterType.HasValue;
        //        }

        //        public bool ShouldSerializeStartDateValue()
        //        {
        //            return StartDateValue.HasValue;
        //        }

        #endregion

        #region Serialize

        //Specific Issues with the serializer with DateTime.Now
        // if you use DateTime.Now it will serialize with the time zone - 2012-01-30T00:00:00-800. Sql Server can't parse it with the time zone...
        // So to fix that please set the date time to too DateTime.UtcNow

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="serializeThisObject">Object to serialize</param>
        /// <returns>String Representation of this object</returns>
        public static string SerializeObject<T>(T serializeThisObject)
        {
            //create the string writer object
            using (var serializeStringWriter = new StringWriter())
            {
                //serialize the object into the string writer
                new XmlSerializer(typeof(T)).Serialize(serializeStringWriter, serializeThisObject);

                //return the string writer
                return serializeStringWriter.ToString();
            }
        }

        /// <summary>
        /// Serialize an object into an XElement Object
        /// </summary>
        /// <param name="serializeThisObject">Object to serialize</param>
        /// <returns>String Representation of this object</returns>
        public static XElement SerializeObjectToXElement<T>(T serializeThisObject)
        {
            //use the other method to grab the xml...this method builds it into a xelement so the user doesn't have to load it themselves for each call
            return XElement.Parse(SerializeObject(serializeThisObject));
        }

        #endregion

        #region Deserializer

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <param name="xmlDataToDeserialize">Serialized Xml Data</param>
        /// <returns>Object Of T</returns>
        public static T DeserializeObject<T>(XElement xmlDataToDeserialize)
        {
            using (var xmlReader = xmlDataToDeserialize.CreateReader())
            {
                return (new XmlSerializer(typeof(T)).Deserialize(xmlReader) ?? throw new Exception("Can't Deserialize Object")).Cast<T>();
            }
        }

        /// <summary>
        /// Deserialize an object straight from a stream. If its byte based you will need to use UTF-8.
        /// </summary>
        /// <returns>Object Of T</returns>
        /// <remarks>Caller should dispose of the stream</remarks>
        public static T DeserializeObject<T>(Stream stream)
        {
            return (new XmlSerializer(typeof(T)).Deserialize(stream) ?? throw new Exception("Can't Deserialize Object")).Cast<T>();
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <param name="xmlDataToDeserialize">Serialized Xml Data</param>
        /// <returns>Object Of T</returns>
        public static T DeserializeObject<T>(string xmlDataToDeserialize)
        {
            //use the overload
            return DeserializeObject<T>(XElement.Parse(xmlDataToDeserialize));
        }

        #endregion

    }
}
