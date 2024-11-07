using System.Diagnostics.CodeAnalysis;
using FastState;

namespace Tokenizer
{
    public class Tokenizer<TTokenType>(IEnumerable<TTokenType> tokenDefinitions)
        : ITokenizer<TTokenType>
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        private readonly StateMachine<TTokenType, char> _stateMachine = TokenizerStateMachineFactory.Create(tokenDefinitions);

        public bool TryParseToken(ReadOnlySpan<char> buffer, bool moreDataAvailable, [MaybeNullWhen(false)] out TTokenType type, out int tokenLength)
        {
            tokenLength = 0;
            TTokenType? tokenType = TokenType<TTokenType>.Start;

            foreach (char c in buffer)
            {
                if (_stateMachine.TryTransition(tokenType, c, out TTokenType newTokenType))
                {
                    tokenType = newTokenType;
                    
                    if (tokenType == TokenType<TTokenType>.EndOfText)
                    {
                        type = TokenType<TTokenType>.Text;
                        return true;
                    }

                    if (tokenType == TokenType<TTokenType>.EndOfWhiteSpace)
                    {
                        type = TokenType<TTokenType>.WhiteSpace;
                        return true;
                    }

                    if (tokenType != TokenType<TTokenType>.WhiteSpace && 
                        tokenType != TokenType<TTokenType>.Text &&
                        tokenType.IsDefined)
                    {
                        type = tokenType;
                        return true;
                    }
                }

                tokenLength++;
            }

            if (tokenType == TokenType<TTokenType>.Start) // If we have more data, check if this current token type is the start of another token type
            {
                tokenLength = 0;
                type = default;
                return false;
            }

            if (moreDataAvailable && _stateMachine.StateHasInputTransitions(tokenType)) 
            {
                tokenLength = 0;
                type = default;
                return false;
            }

            if (_stateMachine.TryGetDefaultForState(tokenType, out TTokenType defaultType))
                tokenType = defaultType;

            if (tokenType == TokenType<TTokenType>.EndOfText)
            {
                type = TokenType<TTokenType>.Text;
                return true;
            }

            if (tokenType == TokenType<TTokenType>.EndOfWhiteSpace)
            {
                type = TokenType<TTokenType>.WhiteSpace;
                return true;
            }

            if (tokenType.IsDefined)
            {
                type = tokenType;
                return true;
            }

            tokenLength = 0;
            type = default;
            return false;
        }
    }
}