using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowHttp.ValueObjects;

namespace FlowHttp.Constants
{
    internal static class HttpContentTypes
    {
        internal static readonly HttpContentType AtomXml = new HttpContentType(HttpContentTypeConstants.AtomXml);
        internal static readonly HttpContentType Css = new HttpContentType(HttpContentTypeConstants.Css);
        internal static readonly HttpContentType Excel = new HttpContentType(HttpContentTypeConstants.Excel);
        internal static readonly HttpContentType FormUrlEncoded = new HttpContentType(HttpContentTypeConstants.FormUrlEncoded);
        internal static readonly HttpContentType GifImage = new HttpContentType(HttpContentTypeConstants.GifImage);
        internal static readonly HttpContentType Html = new HttpContentType(HttpContentTypeConstants.Html);
        internal static readonly HttpContentType JpegImage = new HttpContentType(HttpContentTypeConstants.JpegImage);
        internal static readonly HttpContentType JavaScript = new HttpContentType(HttpContentTypeConstants.JavaScript);
        internal static readonly HttpContentType Json = new HttpContentType(HttpContentTypeConstants.Json);
        internal static readonly HttpContentType MpegAudio = new HttpContentType(HttpContentTypeConstants.MpegAudio);
        internal static readonly HttpContentType Mp4Video = new HttpContentType(HttpContentTypeConstants.Mp4Video);
        internal static readonly HttpContentType OctetStream = new HttpContentType(HttpContentTypeConstants.OctetStream);
        internal static readonly HttpContentType OpenXmlExcel = new HttpContentType(HttpContentTypeConstants.OpenXmlExcel);
        internal static readonly HttpContentType Pdf = new HttpContentType(HttpContentTypeConstants.Pdf);
        internal static readonly HttpContentType PngImage = new HttpContentType(HttpContentTypeConstants.PngImage);
        internal static readonly HttpContentType PlainText = new HttpContentType(HttpContentTypeConstants.PlainText);
        internal static readonly HttpContentType RssXml = new HttpContentType(HttpContentTypeConstants.RssXml);
        internal static readonly HttpContentType Xml = new HttpContentType(HttpContentTypeConstants.Xml);
        internal static readonly HttpContentType XhtmlXml = new HttpContentType(HttpContentTypeConstants.XhtmlXml);
        internal static readonly HttpContentType Zip = new HttpContentType(HttpContentTypeConstants.Zip);
        internal static readonly HttpContentType FormData = new HttpContentType(HttpContentTypeConstants.FormData);
    }
}