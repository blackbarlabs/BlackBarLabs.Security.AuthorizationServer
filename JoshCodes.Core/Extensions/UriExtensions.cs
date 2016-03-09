using System;
using System.Text.RegularExpressions;

namespace JoshCodes.Core.Extensions
{
    public static class UriExtensions
    {
        /// <summary>
        /// Get the name of the file at the end of the path. For example: http://example.com/foo/bar.png?food=barf" would
        /// return "bar.png".
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>File name at end of path or null if no file name is specified.</returns>
        public static string GetFileName(this Uri uri)
        {
            var components = uri.AbsolutePath.Split(new char[] { '/' });
            if (components.Length < 1)
            {
                return null;
            }
            var filename = components[components.Length - 1];
            if (String.IsNullOrWhiteSpace(filename))
            {
                return null;
            }
            return filename;
        }

        public static Uri ChangeFileExtension(this Uri uri, string extension)
        {
            string pattern = @"(.+\.)([^\?]*)([\?].*)?"; // @"(.*[\\/\\A][^\\/]+\\.)([^\\?]*)([\\Z\\?]+.*)";
            string replacement = "$1" + extension + "$3";
            string input = uri.AbsoluteUri;
            string result = Regex.Replace(input, pattern, replacement);
            return new Uri(result);
        }
    }
}
