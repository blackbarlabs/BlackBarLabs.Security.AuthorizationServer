using System;
using System.Collections.Generic;
using System.IO;

namespace JoshCodes.Core.Net
{
    public static class GenericExtensions
    {
        public static IEnumerable<KeyValuePair<string, Stream>> ToStream(this IEnumerable<KeyValuePair<string, Uri>> uriKvps)
        {
            foreach (var uriKvp in uriKvps) 
            {
                var key = uriKvp.Key;
                var uri = uriKvp.Value;
                yield return new KeyValuePair<string, System.IO.Stream>(key, uri.Download());
            }
        }

        public static Stream Download(this Uri url)
        {
            using (var webClient = new System.Net.WebClient())
            {
                var data = webClient.DownloadData(url);
                return new MemoryStream(data);
            }
        }
    }
}
