using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace JoshCodes.Core.Net
{
    public static class ResponseExtensions
    {
        public static bool IsXmlResponse(this HttpWebResponse httpWebResponse)
        {
            return String.Compare("text/xml", httpWebResponse.ContentType, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool TryParseResponseXml(this HttpWebResponse httpWebResponse, out XElement rootElement, bool ignoreContentType = false)
        {
            if (ignoreContentType || httpWebResponse.IsXmlResponse())
            {
                var responseStream = httpWebResponse.GetResponseStream();
                var xdoc = XDocument.Load(responseStream);
                rootElement = xdoc.Root;
                return true;
            }
            rootElement = null;
            return false;
        }

        public static Encoding GetEncoding(this HttpWebResponse httpWebResponse)
        {
            var encodingType = System.Text.Encoding.GetEncodings().FirstOrDefault((encType) =>
                String.Compare(encType.Name, httpWebResponse.ContentEncoding, StringComparison.OrdinalIgnoreCase) == 0);
            var encoding = (encodingType == null) ?
                System.Text.Encoding.ASCII : encodingType.GetEncoding();
            return encoding;
        }

        public static bool TryParseResponseString(this HttpWebResponse httpWebResponse, out string response, bool ignoreContentType = true)
        {
            // TODO: Care about content type
            var encoding = httpWebResponse.GetEncoding();
            var responseStream = httpWebResponse.GetResponseStream();
            using (var memoryStream = new MemoryStream())
            {
                responseStream.CopyTo(memoryStream);
                var data = memoryStream.ToArray();
                var text = encoding.GetString(data);
                response = text;
            }
            return true;
        }
    }
}
