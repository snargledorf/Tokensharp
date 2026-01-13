using System.Buffers;
using System.Diagnostics;

namespace Tokensharp;

internal struct ReadBuffer(int initialBufferSize) : IDisposable
{
    private char[] _buffer = ArrayPool<char>.Shared.Rent(initialBufferSize);
    
    private int _count;
    private bool _endOfReader;

    public readonly bool EndOfReader => _endOfReader;

    public readonly ReadOnlySpan<char> Chars => _buffer.AsSpan(0, _count);

    public readonly async ValueTask<ReadBuffer> ReadAsync(TextReader reader, CancellationToken cancellationToken = default)
    {
        ReadBuffer readBuffer = this;

        do
        {
            int charsRead = await reader.ReadAsync(readBuffer._buffer.AsMemory(readBuffer._count), cancellationToken)
                .ConfigureAwait(false);
            
            if (charsRead == 0)
            {
                readBuffer._endOfReader = true;
                break;
            }
            
            readBuffer._count += charsRead;
            
        } while (readBuffer._count < readBuffer._buffer.Length);

        return readBuffer;
    }

    public void Read(TextReader reader)
    {
        do
        {
            int charsRead = reader.Read(_buffer.AsSpan(_count));
            if (charsRead == 0)
            {
                _endOfReader = true;
                break;
            }

            _count += charsRead;

        } while (_count < _buffer.Length);
    }

    public void AdvanceBuffer(int charsConsumed)
    {
        Debug.Assert(charsConsumed <= _count);
        
        _count -= charsConsumed;

        if (!_endOfReader)
        {
            if ((uint)_count > ((uint)_buffer.Length / 2))
            {
                char[] oldBuffer = _buffer;
                
                int newMinBufferLength = _buffer.Length < (int.MaxValue / 2) ? _buffer.Length * 2 : int.MaxValue;
                char[] newBuffer = ArrayPool<char>.Shared.Rent(newMinBufferLength);
                
                oldBuffer.AsSpan(charsConsumed).CopyTo(newBuffer.AsSpan(0, _count));
                
                _buffer = newBuffer;
                
                ArrayPool<char>.Shared.Return(oldBuffer, true);
            }
            else if (_count != 0)
            {
                _buffer.AsSpan(charsConsumed, _count).CopyTo(_buffer.AsSpan(0, _count));
            }
        }
        else if (_count != 0)
        {
            _buffer.AsSpan(charsConsumed, _count).CopyTo(_buffer.AsSpan(0, _count));
        }
    }

    public void Dispose()
    {
        if (_buffer is null)
            return;
        
        char[] toReturn = _buffer;
        _buffer = null!;
        
        ArrayPool<char>.Shared.Return(toReturn, true);
    }
}