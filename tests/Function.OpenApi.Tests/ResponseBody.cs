namespace Function.OpenApi.Tests;

public record ResponseBody
{
    public required string Name { get; set; }
    public required IEnumerable<int> ListOfInts { get; set; }
    public required NestedClass NestedClass { get; set; }
}