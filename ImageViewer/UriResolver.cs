﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace ImageViewer
{
    internal static class UriResolver
    {
        private static readonly List<string> IsImageList = new List<string>
        {
            ":orig",
            ".jpeg",
            ".jpg",
            ".bmp",
            ".png",
            ".gif",
        };

        public static bool IsImageUri(string uri, out string imageUri)
        {
            var result = false;
            string targetUri = null;
            IsImageList.ForEach(x =>
            {
                if (uri.EndsWith(x))
                {
                    targetUri = uri;
                    result = true;
                }
            });

            if (result == false)
            {
                string apiResult;
                if (GetAzyobuziApiResult(uri, out apiResult))
                {
                    targetUri = apiResult;
                    result = true;
                }
            }

            imageUri = targetUri;
            return result;
        }

        private static bool GetAzyobuziApiResult(string uri, out string resultUri)
        {
            const string ApiBaseUri = @"http://img.azyobuzi.net/api/all_sizes.json?uri=";

            var req = WebRequest.Create(ApiBaseUri + uri);
            req.Timeout = 3000;

            try
            {
                var res = (HttpWebResponse) req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    using (var resStream = res.GetResponseStream())
                    using (var sr = new StreamReader(resStream, Encoding.UTF8))
                    {
                        var resBody = sr.ReadToEnd();
                        resultUri = GetImageUriOfApiResult(resBody);
                        return true;
                    }
                }
                resultUri = null;
                return false;
            }
            catch
            {
                resultUri = null;
                return false;
            }
        }

        private static string GetImageUriOfApiResult(string resultJson)
        {
            var jsonValue = new List<Json>();
            var jsonRoot = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(resultJson);
            jsonRoot.ToList()
                .ForEach(x => jsonValue.Add(new Json {Name = x.Key, Value = x.Value == null ? "" : x.Value.ToString()}));
            return jsonValue.Where(x => x.Name == "full").ToList()[0].Value;
        }

        private class Json
        {
            public string Name { get; set; }

            public string Value { get; set; }
        }
    }
}