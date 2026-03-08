using System.Net;

namespace Function.OpenApi;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class OpenApiResponseAttribute(string contentType, Type bodyType, HttpStatusCode httpStatusCode) : Attribute
{
    public string ContentType => contentType;
    public Type BodyType => bodyType;
    public HttpStatusCode HttpStatusCode => httpStatusCode;
}
