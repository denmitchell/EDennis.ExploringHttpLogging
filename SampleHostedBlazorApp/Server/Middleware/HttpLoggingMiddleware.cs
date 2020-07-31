using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SampleHostedBlazorApp.Server.Middleware {
    public class HttpLoggingMiddleware {

        private readonly RequestDelegate next;
        private readonly ILogger<HttpLoggingMiddleware> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public HttpLoggingMiddleware(RequestDelegate next, 
            ILogger<HttpLoggingMiddleware> logger) {
            this.next = next;
            _logger = logger;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task InvokeAsync(HttpContext context) {
            // create a new log object
            var log = new HttpLog {
                Path = context.Request.Path,
                Method = context.Request.Method,
                QueryString = context.Request.QueryString.ToString(),
                DisplayUrl = context.Request.GetDisplayUrl()
            };

            if ((context.Request.Method == "POST" || context.Request.Method == "PUT" 
                || context.Request.Method == "PATCH") 
                && context.Request.ContentLength != default 
                && context.Request.ContentLength > 0) { 
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body)
                                                    .ReadToEndAsync();
                context.Request.Body.Position = 0;
                log.Payload = body;
            }

            log.RequestedOn = DateTime.Now;

            var originalBodyStream = context.Response.Body;

            await using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;

            await next.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            log.Response = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            log.ResponseCode = context.Response.StatusCode.ToString();

            using (_logger.BeginScope(new Dictionary<string, object> {
                            { "Path", log.Path },
                            { "QueryString", log.QueryString },
                            { "DisplayUrl", log.DisplayUrl },
                            { "Method", log.Method },
                            { "RequestedOn", log.RequestedOn },
                            { "RespondedOn", log.RespondedOn },
                            { "ResponseCode", log.ResponseCode },
                            { "RequestBody", log.Payload },
                            { "ResponseBody", log.Response }})) {
                _logger.LogInformation("Http Request and Response logged for {log.DisplayUrl}", log.DisplayUrl);
            }

            await responseBody.CopyToAsync(originalBodyStream);


            //await next.Invoke(context);

            //using Stream originalRequest = context.Response.Body;
            //try {
            //    using var memStream = new MemoryStream();
            //    context.Response.Body = memStream;
            //    // All the Request processing as described above 
            //    // happens from here.
            //    // Response handling starts from here
            //    // set the pointer to the beginning of the 
            //    // memory stream to read
            //    memStream.Position = 0;
            //    // read the memory stream till the end
            //    var response = await new StreamReader(memStream)
            //                                            .ReadToEndAsync();
            //    // write the response to the log object
            //    log.Response = response;
            //    log.ResponseCode = context.Response.StatusCode.ToString();
            //    log.IsSuccessStatusCode = (
            //          context.Response.StatusCode == 200 ||
            //          context.Response.StatusCode == 201);
            //    log.RespondedOn = DateTime.Now;

            //    // add the log object to the logger stream 
            //    // via the Repo instance injected
            //    using (_logger.BeginScope(new Dictionary<string, object> {
            //                { "Path", log.Path },
            //                { "QueryString", log.QueryString },
            //                { "DisplayUrl", log.DisplayUrl },
            //                { "Method", log.Method },
            //                { "RequestedOn", log.RequestedOn },
            //                { "RespondedOn", log.RespondedOn },
            //                { "ResponseCode", log.ResponseCode },
            //                { "RequestBody", log.Payload },
            //                { "ResponseBody", log.Response }})) {
            //        _logger.LogInformation("Http Request and Response logged for {log.DisplayUrl}", log.DisplayUrl);
            //    }


            //    // since we have read till the end of the stream, 
            //    // reset it onto the first position
            //    memStream.Position = 0;

            //    // now copy the content of the temporary memory 
            //    // stream we have passed to the actual response body 
            //    // which will carry the response out.
            //    await memStream.CopyToAsync(originalRequest);
            //} catch (Exception ex) {
            //    Console.WriteLine(ex);
            //} finally {
            //    // assign the response body to the actual context
            //    context.Response.Body = originalRequest;
            //}



        }


    }

    public static class IApplicationBuilderExtensions {
        public static IApplicationBuilder UseHttpLogging(this IApplicationBuilder builder) {
            builder.UseMiddleware<HttpLoggingMiddleware>();
            return builder;
        }

    }

}
