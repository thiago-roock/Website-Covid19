using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace Covid19.Domain.Model
{
    public class CrawlerXmlConvert
    {
        public CrawlerXmlConvert() { }

        public string SerializeObject<T>(T dataObject)
        {
            if (dataObject == null)
            {
                return string.Empty;
            }
            try
            {
                using (StringWriter stringWriter = new System.IO.StringWriter())
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(stringWriter, dataObject);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public T DeserializeObject<T>(string xml)
             where T : new()
        {
            if (string.IsNullOrEmpty(xml))
            {
                return new T();
            }
            try
            {
                using (var stringReader = new StringReader(xml))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(stringReader);
                }
            }
            catch (Exception ex)
            {
                return new T();
            }
        }
        public string GetWebText(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.UserAgent = "A .NET Web Crawler";

            WebResponse response = request.GetResponse();

            Stream stream = response.GetResponseStream();

            StreamReader reader = new StreamReader(stream);
            string htmlText = reader.ReadToEnd();
            return htmlText;
        }

    }
}
