// BY COPILOT
namespace Function.OpenApi.Tests;

public record ByteTestModel
{
    public byte SingleByte { get; init; }
    public byte[]? BinaryData { get; init; }
    public byte? NullableByte { get; init; }
    public byte[]? NullableBinaryData { get; init; }
}
