namespace Function.OpenApi.Tests;

public record RequestBody
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required int Age { get; init; }
    public required DateOnly DateOnly { get; init; }
    public required List<string> ListOfStrings { get; init; }
    public required RequestKind Kind { get; init; }
}

public enum RequestKind
{
    Standard,
    Urgent
}