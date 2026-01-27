using System.Diagnostics.CodeAnalysis;

namespace Tokensharp;

public ref struct TokenReader<TTokenType>(
    ReadOnlySpan<char> buffer,
    TokenConfiguration<TTokenType> tokenConfiguration,
    bool moreDataAvailable = false,
    TokenReaderOptions options = default)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly TokenParser<TTokenType> _tokenParser = new(tokenConfiguration);

    private readonly ReadOnlySpan<char> _buffer = buffer;

    public TokenReader(ReadOnlySpan<char> buffer,
        bool moreDataAvailable = false,
        TokenReaderOptions options = default) : this(buffer, TTokenType.Configuration, moreDataAvailable, options)
    {
    }

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
        index = Consumed;
        
        TokenParser<TTokenType> tokenParser = _tokenParser;

        while (tokenParser.TryParseToken(_buffer[index..], moreDataAvailable, out tokenType, out length))
        {
            Consumed += length;

            if (!options.IgnoreWhiteSpace || tokenType != TokenType<TTokenType>.WhiteSpace)
                return true;

            index = Consumed;
        }

        return false;
    }
}