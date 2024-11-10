using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FastState;

namespace Tokensharp;

public ref struct TokenReader<TTokenType>(ReadOnlySpan<char> buffer, bool moreDataAvailable = false, TokenReaderOptions options = default)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static readonly StateMachine<TTokenType, char> StateMachine;
    
    private readonly ReadOnlySpan<char> _buffer = buffer;

    static TokenReader() => StateMachine = TokenizerStateMachine<TTokenType>.Instance;

    public int Consumed { get; private set; }

    public bool Read([MaybeNullWhen(false)] out TTokenType tokenType, out ReadOnlySpan<char> lexeme)
    {
        while (ReadInternal(out tokenType, out lexeme))
        {
            if (options.IgnoreWhiteSpace && tokenType == TokenType<TTokenType>.WhiteSpace)
                continue;
            
            return true;
        }
        
        return false;
    }

    private bool ReadInternal([MaybeNullWhen(false)] out TTokenType tokenType, out ReadOnlySpan<char> lexeme)
    {
        tokenType = default;
        int lexemeLength = 0;

        TTokenType? currentTokenType = TokenType<TTokenType>.Start;
        
        try
        {
            foreach (char c in _buffer[Consumed..])
            {
                if (StateMachine.TryTransition(currentTokenType, c, out TTokenType newTokenType))
                {
                    currentTokenType = newTokenType;

                    if (currentTokenType == TokenType<TTokenType>.EndOfText)
                    {
                        tokenType = TokenType<TTokenType>.Text;
                        return true;
                    }

                    if (currentTokenType == TokenType<TTokenType>.EndOfNumber)
                    {
                        tokenType = TokenType<TTokenType>.Number;
                        return true;
                    }

                    if (currentTokenType == TokenType<TTokenType>.EndOfWhiteSpace)
                    {
                        tokenType = TokenType<TTokenType>.WhiteSpace;
                        return true;
                    }

                    if (currentTokenType != TokenType<TTokenType>.Text &&
                        currentTokenType != TokenType<TTokenType>.Number &&
                        currentTokenType != TokenType<TTokenType>.WhiteSpace &&
                        !currentTokenType.IsGenerated)
                    {
                        tokenType = currentTokenType;
                        return true;
                    }
                }

                lexemeLength++;
            }

            if (currentTokenType == TokenType<TTokenType>.Start)
            {
                Debug.Assert(tokenType == default);
                return false;
            }
            
            // If we have more data, check if this current token type is the start of another token type
            // If it is, then we should wait for more data to be available
            if (moreDataAvailable && StateMachine.StateHasInputTransitions(currentTokenType))
            {
                Debug.Assert(tokenType == default);
                return false;
            }

            if (StateMachine.TryGetDefaultForState(currentTokenType, out TTokenType defaultType))
                currentTokenType = defaultType;

            if (currentTokenType == TokenType<TTokenType>.EndOfText)
            {
                tokenType = TokenType<TTokenType>.Text;
                return true;
            }

            if (currentTokenType == TokenType<TTokenType>.EndOfNumber)
            {
                tokenType = TokenType<TTokenType>.Number;
                return true;
            }

            if (currentTokenType == TokenType<TTokenType>.EndOfWhiteSpace)
            {
                tokenType = TokenType<TTokenType>.WhiteSpace;
                return true;
            }

            if (!currentTokenType.IsGenerated)
            {
                tokenType = currentTokenType;
                return true;
            }

            Debug.Assert(tokenType == default);
            return false;
        }
        finally
        {
            lexeme = _buffer.Slice(Consumed, lexemeLength);
            Consumed += lexemeLength;
        }
    }
}