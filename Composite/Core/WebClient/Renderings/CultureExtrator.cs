﻿using System;
using System.Globalization;
using System.Linq;
using Composite.Data;
using Composite.Core.WebClient;


namespace Composite.Core.WebClient.Renderings
{
    [Obsolete("No longer used")]
    internal static class CultureExtrator
    {
        public static CultureInfo GetCultureInfo(string requestPath)
        {
            string newRequestPath;

            return GetCultureInfo(requestPath, out newRequestPath);
        }


        public static CultureInfo GetCultureInfo(string requestPath, out string requestPathWithoutUrlMappingName)
        {
            requestPathWithoutUrlMappingName = requestPath;

            int startIndex = requestPath.IndexOf('/', UrlUtils.PublicRootPath.Length) + 1;
            if (startIndex >= 0)
            {
                int endIndex = requestPath.IndexOf('/', startIndex) - 1;
                if (endIndex >= 0)
                {
                    string urlMappingName = requestPath.Substring(startIndex, endIndex - startIndex + 1).ToLowerInvariant();

                    if (DataLocalizationFacade.UrlMappingNames.Contains(urlMappingName))
                    {
                        CultureInfo cultureInfo = DataLocalizationFacade.GetCultureInfoByUrlMappingName(urlMappingName);

                        bool exists = DataLocalizationFacade.ActiveLocalizationNames.Contains(cultureInfo.Name);

                        if (exists)
                        {
                            requestPathWithoutUrlMappingName = requestPath.Remove(startIndex - 1, endIndex - startIndex + 2);

                            return cultureInfo;
                        }
                        return null;
                    }
                }
            }

            return DataLocalizationFacade.DefaultUrlMappingCulture;
        }
    }
}
