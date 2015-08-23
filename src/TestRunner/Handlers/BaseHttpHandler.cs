namespace NUnitContrib.Web.TestRunner.Handlers
{
    using System;
    using System.Collections.Concurrent;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Web;
    using System.Web.SessionState;
    using System.Web.Routing;

    public abstract class BaseHttpHandler : IHttpHandler, IRouteHandler, IReadOnlySessionState
    {
        private readonly ConcurrentDictionary<string, string> resourceCache = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, byte[]> imageCache = new ConcurrentDictionary<string, byte[]>();

        public virtual bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        protected void ReturnResponse(HttpContextBase context, string message, string contentType = "text/plain", HttpStatusCode status = HttpStatusCode.OK, bool endResponse = false)
        {
            if (!String.IsNullOrWhiteSpace(message)) context.Response.Write(message);
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = contentType;
            if (endResponse) context.Response.End();
        }

        protected void NotFound(HttpContextBase context, string message = null)
        {
            ReturnResponse(context, message, status: HttpStatusCode.NotFound);
        }

        protected void ReturnResource(HttpContextBase context, string file, string contentType)
        {
            ReturnResponse(context, GetResource(file), contentType);
        }

        protected void ReturnImage(HttpContextBase context, string file, ImageFormat imageFormat, string contentType)
        {
            var buffer = GetImage(file, imageFormat);
            context.Response.ContentType = "image/png";
            context.Response.BinaryWrite(buffer);
            context.Response.Flush();
        }

        protected void ReturnJson(HttpContextBase context, object o)
        {
            var serializer = new DataContractJsonSerializer(o.GetType());
            var json = string.Empty;
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, o);
                json = Encoding.Default.GetString(ms.ToArray());
            }
            context.Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
            context.Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
            context.Response.AppendHeader("Expires", "0"); // Proxies.
            ReturnResponse(context, json, "application/json");
        }

        private string GetResource(string filename)
        {
            filename = filename.ToLowerInvariant();
            string result;
            if (resourceCache.TryGetValue(filename, out result)) return result;
            using (var stream = typeof(RunnerHandler).Assembly.GetManifestResourceStream("NUnitContrib.Web.TestRunner.Resources." + filename))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }

            resourceCache[filename] = result;
            return result;
        }

        private byte[] GetImage(string filename, ImageFormat imageFormat)
        {
            filename = filename.ToLowerInvariant();
            byte[] result;
            if (imageCache.TryGetValue(filename, out result)) return result;
            using (var stream = typeof(RunnerHandler).Assembly.GetManifestResourceStream("NUnitContrib.Web.TestRunner.Resources." + filename))
            {
                if (stream != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        var bmp = new Bitmap(stream);
                        bmp.Save(ms, imageFormat);
                        result = ms.ToArray();
                    }
                }
            }

            imageCache[filename] = result;
            return result;
        }

        public abstract void ProcessRequest(HttpContextBase context);

        public IHttpHandler GetHttpHandler(RequestContext requestContext) { return this; }
    }
}
