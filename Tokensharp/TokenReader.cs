using System.Diagnostics.CodeAnalysis;
using Tokensharp.StateMachine;

namespace Tokensharp;

public ref struct TokenReader<TTokenType>(
    ReadOnlySpan<char> buffer,
    TokenReaderStateMachine<TTokenType> stateMachine,
    bool moreDataAvailable = false,
    TokenReaderOptions options = default)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly TokenParser<TTokenType> _tokenParser = new(stateMachine);
    private readonly ReadOnlySpan<char> _buffer = buffer;

    public int Consumed { get; private set; }

    public bool Read([NotNullWhen(true)] out TokenType<TTokenType>? tokenType, out ReadOnlySpan<char> lexeme)
    {
        if (Read(out tokenType, out int index, out int length))
        {
            lexeme = _buffer.Slice(index, length);
            return true;
        }

        lexeme = default;
        return false;
    }

    public bool Read([NotNullWhen(true)] out TokenType<TTokenType>? tokenType, out int index, out int length)
    {
        tokenType = null;
        index = Consumed;
        length = 0;
        
        TokenParser<TTokenType> tokenParser = _tokenParser;

        while (tokenParser.TryParseToken(_buffer[Consumed..], moreDataAvailable, out tokenType, out int potentialLength))
        {
            if (!options.IgnoreWhiteSpace || tokenType != TokenType<TTokenType>.WhiteSpace)
            {
                length = potentialLength;
                break;
            }

            Consumed += potentialLength;
            index = Consumed;
        }
        
        Consumed += length;
        return tokenType is not null;
    }
}