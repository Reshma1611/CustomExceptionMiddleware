using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace CustomExceptionMiddleware
{
    public static class ExceptionMiddleware
    {
        public static JsonSerializerSettings SerializerSettingsNewtonSoftJson
        {
            get
            {
                return new JsonSerializerSettings
                {                   
                    NullValueHandling = NullValueHandling.Ignore,
                };
            }
        }
        public static string ToJsonString<T>(this T obj, Formatting formatting = Formatting.Indented) => JsonConvert.SerializeObject(obj, formatting, SerializerSettingsNewtonSoftJson);
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            var config = (app as WebApplication)?.Configuration;
            var env = (app as WebApplication)?.Environment;

            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    string dataToSend = string.Empty;
                    var isProduction = env.IsEnvironment("production");
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        context.Response.ContentType = "application/json";
                        if (contextFeature.Error is ApplicationError customException)
                        {
                            context.Response.StatusCode = (int)customException.StatusCode;
                            if (customException.FieldError != null)
                            {
                                dataToSend = new ResponseData<List<FieldError>>() { Data = new List<FieldError>() { customException.FieldError } }.ToJsonString();
                            }
                            else
                            {
                                dataToSend = new ResponseData() { Message = customException.Message }.ToJsonString();
                            }
                            if (customException.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                //log the error
                            }
                        }
                        else
                        {
                            if (isProduction)
                            {
                                dataToSend = new ResponseData() { Message = "Some error ocuure while processing your request" }.ToJsonString();
                            }                            
                            else
                            {
                                dataToSend = new ResponseData<Exception>() { Data = contextFeature.Error }.ToJsonString();
                            }
                        }

                    }
                    await context.Response.WriteAsync(dataToSend);
                });
            });
        }
    }
    public class ApplicationError : Exception
    {
        public readonly HttpStatusCode StatusCode = HttpStatusCode.InternalServerError;
        public readonly FieldError? FieldError = null;

        public ApplicationError(FieldError fe) : base(fe.Errors?.FirstOrDefault() ?? "Validation error.")
        {
            FieldError = fe;
            StatusCode = HttpStatusCode.BadRequest;
        }
        //id is required on update
        public ApplicationError(string fieldName, string error) : base(error ?? "Validation error.")
        {
            FieldError = new FieldError(fieldName, error ?? "");
            StatusCode = HttpStatusCode.BadRequest;
        }
        public ApplicationError(string fieldName, string[] errors) : base(errors?.FirstOrDefault() ?? "Validation error.")
        {
            FieldError = new FieldError(fieldName, errors ?? new string[] { "" });
            StatusCode = HttpStatusCode.BadRequest;
        }
        //email not found
        public ApplicationError(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
        //not found
        public ApplicationError(string message) : base(message)
        {

        }
    }
    public class FieldError
    {
        public string Property { get; set; }
        public string[]? Errors { get; set; }
        public FieldError(string property, string error)
        {
            Property = property;
            Errors = new string[] { error };
        }
        public FieldError(string property, string[] errors)
        {
            Property = property;
            Errors = errors;
        }
    }
    public class ResponseData<T>
    {
        public int? Status { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
    public class ResponseData
    {
        public int? Status { get; set; }
        public string? Message { get; set; }
    }
}
