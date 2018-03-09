using System;
using System.IO;
using System.Net;

public class NetworkUtils
{
    public static Boolean UrlExists(string url)
    {
        string mimeType = null;
        string charset = null;
        try
        {
            HttpWebRequest req = PrepareRequest(url);
            req.Method = "HEAD";
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            using (Stream resst = resp.GetResponseStream())
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    ExtractCharsetAndMimeType(resp, out mimeType, out charset);
                    if (mimeType.Length > 0)
                        return true;
                }
            req = PrepareRequest(url);
            req.Method = "GET";
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            using (Stream resst = resp.GetResponseStream())
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    ExtractCharsetAndMimeType(resp, out mimeType, out charset);
                    if (mimeType.Length > 0)
                        return true;
                }
        }
        catch { }
        return false;
    }

    private static void ExtractCharsetAndMimeType(HttpWebResponse resp
        , out string mimeType
        , out string charset)
    {
        mimeType = string.Empty;
        charset = string.Empty;
        if (!String.IsNullOrEmpty(resp.CharacterSet))
            charset = resp.CharacterSet;
        if (!String.IsNullOrEmpty(resp.ContentType)
            && resp.ContentType.Trim().Length > 0)
        {
            int index = resp.ContentType.IndexOf(';');
            if (index > -1)
                mimeType = resp.ContentType.Substring(0, index);
            else
                mimeType = resp.ContentType;
            mimeType = mimeType.Trim();
        }
    }

    private static HttpWebRequest PrepareRequest(string url)
    {
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
        req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.1.4322; .NET CLR 2.0.40607)";
        req.AllowAutoRedirect = true;
        req.AutomaticDecompression = DecompressionMethods.Deflate
            | DecompressionMethods.GZip;
        req.MaximumAutomaticRedirections = 5;
        return req;
    }
}
