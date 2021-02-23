using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace Readarr.Http.Extensions
{
    public static class RequestExtensions
    {
        public static bool IsApiRequest(this HttpRequest request)
        {
            return request.Path.StartsWithSegments("/api", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsFeedRequest(this HttpRequest request)
        {
            return request.Path.StartsWithSegments("/feed", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsSignalRRequest(this HttpRequest request)
        {
            return request.Path.StartsWithSegments("/signalr", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsLocalRequest(this HttpRequest request)
        {
            return request.Host.Equals("localhost") ||
                    request.Host.Equals("127.0.0.1") ||
                    request.Host.Equals("::1");
        }

        public static bool IsLoginRequest(this HttpRequest request)
        {
            return request.Path.Equals("/login", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsContentRequest(this HttpRequest request)
        {
            return request.Path.StartsWithSegments("/Content", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool GetBooleanQueryParameter(this HttpRequest request, string parameter, bool defaultValue = false)
        {
            var parameterValue = request.Query[parameter];

            if (parameterValue.Any())
            {
                return bool.Parse(parameterValue.ToString());
            }

            return defaultValue;
        }

        public static bool IsSharedContentRequest(this HttpRequest request)
        {
            return request.Path.StartsWithSegments("/MediaCover", StringComparison.InvariantCultureIgnoreCase) ||
                   request.Path.StartsWithSegments("/Content/Images", StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GetRemoteIP(this HttpContext context)
        {
            return context?.Request?.GetRemoteIP() ?? "Unknown";
        }

        public static string GetRemoteIP(this HttpRequest request)
        {
            if (request == null)
            {
                return "Unknown";
            }

            var remoteIP = request.HttpContext.Connection.RemoteIpAddress;
            var remoteAddress = remoteIP.ToString();

            // Only check if forwarded by a local network reverse proxy
            if (remoteIP.IsLocalAddress())
            {
                var realIPHeader = request.Headers["X-Real-IP"];
                if (realIPHeader.Any())
                {
                    return realIPHeader.First().ToString();
                }

                var forwardedForHeader = request.Headers["X-Forwarded-For"];
                if (forwardedForHeader.Any())
                {
                    // Get the first address that was forwarded by a local IP to prevent remote clients faking another proxy
                    foreach (var forwardedForAddress in forwardedForHeader.SelectMany(v => v.Split(',')).Select(v => v.Trim()).Reverse())
                    {
                        if (!IPAddress.TryParse(forwardedForAddress, out remoteIP))
                        {
                            return remoteAddress;
                        }

                        if (!remoteIP.IsLocalAddress())
                        {
                            return forwardedForAddress;
                        }

                        remoteAddress = forwardedForAddress;
                    }
                }
            }

            return remoteAddress;
        }

        public static void DisableCache(this IHeaderDictionary headers)
        {
            headers["Cache-Control"] = "no-cache, no-store";
            headers["Expires"] = "-1";
            headers["Pragma"] = "no-cache";
        }

        public static void EnableCache(this IHeaderDictionary headers)
        {
            headers["Cache-Control"] = "max-age=31536000, public";
            headers["Last-Modified"] = BuildInfo.BuildDateTime.ToString("r");
        }
    }
}
