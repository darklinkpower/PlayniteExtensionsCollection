using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowHttp.ValueObjects;

namespace FlowHttp.Constants
{
    public static class HttpContentTypes
    {
        public static readonly HttpContentType AtomXml = new HttpContentType(HttpContentTypeConstants.AtomXml);
        public static readonly HttpContentType Css = new HttpContentType(HttpContentTypeConstants.Css);
        public static readonly HttpContentType Excel = new HttpContentType(HttpContentTypeConstants.Excel);
        public static readonly HttpContentType FormUrlEncoded = new HttpContentType(HttpContentTypeConstants.FormUrlEncoded);
        public static readonly HttpContentType GifImage = new HttpContentType(HttpContentTypeConstants.GifImage);
        public static readonly HttpContentType Html = new HttpContentType(HttpContentTypeConstants.Html);
        public static readonly HttpContentType JpegImage = new HttpContentType(HttpContentTypeConstants.JpegImage);
        public static readonly HttpContentType JavaScript = new HttpContentType(HttpContentTypeConstants.JavaScript);
        public static readonly HttpContentType Json = new HttpContentType(HttpContentTypeConstants.Json);
        public static readonly HttpContentType MpegAudio = new HttpContentType(HttpContentTypeConstants.MpegAudio);
        public static readonly HttpContentType Mp4Video = new HttpContentType(HttpContentTypeConstants.Mp4Video);
        public static readonly HttpContentType OctetStream = new HttpContentType(HttpContentTypeConstants.OctetStream);
        public static readonly HttpContentType OpenXmlExcel = new HttpContentType(HttpContentTypeConstants.OpenXmlExcel);
        public static readonly HttpContentType Pdf = new HttpContentType(HttpContentTypeConstants.Pdf);
        public static readonly HttpContentType PngImage = new HttpContentType(HttpContentTypeConstants.PngImage);
        public static readonly HttpContentType PlainText = new HttpContentType(HttpContentTypeConstants.PlainText);
        public static readonly HttpContentType RssXml = new HttpContentType(HttpContentTypeConstants.RssXml);
        public static readonly HttpContentType Xml = new HttpContentType(HttpContentTypeConstants.Xml);
        public static readonly HttpContentType XhtmlXml = new HttpContentType(HttpContentTypeConstants.XhtmlXml);
        public static readonly HttpContentType Zip = new HttpContentType(HttpContentTypeConstants.Zip);
        public static readonly HttpContentType FormData = new HttpContentType(HttpContentTypeConstants.FormData);
    }
}