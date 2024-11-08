using System.Buffers;
using System.Diagnostics;

namespace Tokensharp;

internal struct ReadBuffer<TTokenType> : IDisposable where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private char[] _buffer;
    
    private int _offset;
    private int _count;
    private bool _endOfReader;

    public ReadBuffer(int initialBufferSize)
    {
        int largestLexeme = TTokenType.TokenTypes.Any()
            ? TTokenType.TokenTypes.Max(tt => tt.Lexeme.Length)
            : 0;
        
        _buffer = ArrayPool<char>.Shared.Rent(Math.Max(largestLexeme, initialBufferSize));
    }

    public readonly bool EndOfReader => _endOfReader;

    public readonly ReadOnlySpan<char> Chars => _buffer.AsSpan(_offset, _count);

    public readonly async ValueTask<ReadBuffer<TTokenType>> ReadAsync(TextReader reader, CancellationToken cancellationToken = default)
    {
        ReadBuffer<TTokenType> readBuffer = this;

        do
        {
            int charsRead = await reader.ReadAsync(readBuffer._buffer.AsMemory(_count), cancellationToken).ConfigureAwait(false);
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
            if (_count > _buffer.Length / 2)
            {
                char[] oldBuffer = _buffer;
                
                int newMinBufferLength = _buffer.Length < (int.MaxValue / 2) ? _buffer.Length * 2 : int.MaxValue;
                char[] newBuffer = ArrayPool<char>.Shared.Rent(newMinBufferLength);
                
                Buffer.BlockCopy(oldBuffer, _offset + charsConsumed, newBuffer, 0, _count);
                
                _buffer = newBuffer;
                
                ArrayPool<char>.Shared.Return(oldBuffer, true);
            }
            else if (_count != 0)
            {
                Buffer.BlockCopy(_buffer, _offset + charsConsumed, _buffer, 0, _count);
            }
        }
        
        _offset = 0;
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