namespace Function.OpenApi;

public enum OpenApiHeaderDirection
{
    In,
    Out
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class OpenApiHeaderAttribute(string name, Type headerType, OpenApiHeaderDirection direction, bool required = false) : Attribute
{
    public string Name => name;
    public Type HeaderType => headerType;
    public OpenApiHeaderDirection Direction => direction;
    public bool Required => required;

    public int ResponseStatusCode { get; set; }
}
