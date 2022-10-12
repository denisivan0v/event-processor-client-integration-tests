using System.Buffers;

namespace SampleApp;

public interface IDataStorage
{
    void Buffer(ReadOnlyMemory<byte> data);
}

public class DataStorage : IDataStorage
{
    private readonly ArrayBufferWriter<byte> _bufferWriter = new();

    public void Buffer(ReadOnlyMemory<byte> data)
    {
        var span = _bufferWriter.GetSpan(data.Length);
        data.Span.CopyTo(span);
        _bufferWriter.Advance(data.Length);
    }
}