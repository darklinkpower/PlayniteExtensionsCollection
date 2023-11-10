using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCommon
{
    public static class CookiesUtilities
    {
        /// <summary>
        /// Converts the specified string representation of an HTTP cookie to Cookie
        /// </summary>
        /// <param name="cookieString"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static bool TryParseCookieFromString(string cookieString, out Cookie cookie)
        {
            cookie = new Cookie();
            if (string.IsNullOrEmpty(cookieString))
            {
                return false;
            }

            string[] cookieParts = cookieString.Split(';');
            var cookiePart = 0;
            foreach (var part in cookieParts)
            {
                cookiePart++;
                string[] keyValue = part.Trim().Split('=');
                var key = keyValue[0].Trim();
                var value = keyValue.Length > 1 ? keyValue[1].Trim() : string.Empty;

                if (cookiePart == 1 && keyValue.Length == 2)
                {
                    cookie.Name = key;
                    cookie.Value = value;
                    continue;
                }

                switch (key.ToLower())
                {
                    case "expires":
                        if (DateTime.TryParseExact(value, "r", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expiryDate))
                        {
                            cookie.Expires = expiryDate.ToUniversalTime();
                        }
                        break;
                    case "Max-Age":
                        if (int.TryParse(value, out int maxAge))
                        {
                            cookie.Expires = DateTime.Now.AddSeconds(maxAge);
                        }
                        break;
                    case "path":
                        cookie.Path = value;
                        break;
                    case "secure":
                        cookie.Secure = true;
                        break;
                    case "httponly":
                        cookie.HttpOnly = true;
                        break;
                    case "samesite":
                        break;
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(cookie.Name))
            {
                return true; // If cookie name was obtained, it was successfull
            }

            return false;
        }


    }
}