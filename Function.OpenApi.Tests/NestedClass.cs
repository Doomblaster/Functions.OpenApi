namespace Function.OpenApi.Tests;

public record NestedClass
{
    public Guid? Id { get; init; }
    public string? Name { get; init; }
    public int? Age { get; init; }
    public DateOnly? DateOnly { get; init; }
    public long LongInteger { get; init; }
    public float FloatNumber { get; init; }
}