namespace Function.OpenApi;

public class OpenApiRequestBodyAttribute(string contentType, Type bodyType) : Attribute
{
    public string ContentType => contentType;
    public Type BodyType => bodyType;
}