using System;

namespace JoshCodes.Core.Urn
{
    public static class UrnFactory
    {
        public static Uri CreateUrn(string ns, params object[] args)
        {
			string [] strArgs = (string[])args;
			var nsParams = String.Join(":", strArgs);
            return new Uri(String.Format("urn:{0}:{1}", ns, nsParams));
        }
    }      
}

