using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SwiftState;
using Tokensharp.StateMachine;

namespace Tokensharp;

public readonly ref struct TokenParser<TTokenType>(TokenParserStateMachine<TTokenType> stateMachine) 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public bool TryParseToken(ReadOnlySpan<char> buffer, bool moreDataAvailable,
        [NotNullWhen(true)] out TokenType<TTokenType>? tokenType, out ReadOnlySpan<char> lexeme)
    {
        if (TryParseToken(buffer, moreDataAvailable, out tokenType, out int length))
        {
            lexeme = buffer[..length];
            return true;
        }

        tokenType = null;
        lexeme = default;
        return false;
    }
    
    public bool TryParseToken(ReadOnlySpan<char> buffer, bool moreDataAvailable, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType, out int length)
    {
        tokenType = null;
        length = 0;

        State<char, TokenizerStateId<TTokenType>>? previousLongestState = null;
        int previousLongestLexemeLength = 0;
        int characterCount = 0;

        State<char, TokenizerStateId<TTokenType>> currentState = stateMachine.StartState;

        foreach (char c in buffer)
        {
            if (currentState.TryTransition(c, out State<char, TokenizerStateId<TTokenType>>? newState))
            {
                State<char, TokenizerStateId<TTokenType>> previousState = currentState;
                currentState = newState;

                if (currentState.IsTerminal)
                {
                    if (previousLongestState is not null && (!previousLongestState.Id.TokenType.IsUserDefined ||
                                                             currentState.Id.TokenType ==
                                                             previousLongestState.Id.TokenType))
                    {
                        tokenType = previousLongestState.Id.TokenType;
                        length = previousLongestLexemeLength;
                    }
                    else
                    {
                        tokenType = currentState.Id.TokenType;
                        length = characterCount;
                    }

                    break;
                }

                if (previousState != stateMachine.StartState && previousState.Id.TokenType != currentState.Id.TokenType)
                {
                    // If our new state matches our previous longest, then we've fallen back to the previous token type
                    // This is most common when parsing text when a potential token fails and falls back to text.
                    if (previousLongestState == currentState)
                    {
                        previousLongestState = null;
                        previousLongestLexemeLength = 0;
                    }
                    else
                    {
                        if (previousLongestState is not null && (!previousLongestState.Id.TokenType.IsUserDefined ||
                                                                 currentState.Id.TokenType ==
                                                                 previousLongestState.Id.TokenType))
                        {
                            tokenType = previousLongestState.Id.TokenType;
                            length = previousLongestLexemeLength;
                            break;
                        }

                        previousLongestState = previousState;
                        previousLongestLexemeLength = characterCount;
                    }
                }
            }

            characterCount++;
        }

        // If we are currently parsing a token but there is no more data, then we should check
        // if our current state (or it's default transition) is a terminal state.
        if (tokenType is null && currentState.Id != TokenizerStateId<TTokenType>.Start && !moreDataAvailable)
        {
            State<char, TokenizerStateId<TTokenType>> previousState = currentState;
            
            // Check if our current state has a default state
            if (currentState.TryGetDefault(out State<char, TokenizerStateId<TTokenType>>? defaultState))
                currentState = defaultState;
            
            if (currentState.IsTerminal)
            {
                // Check if our current state is the terminal for the previously parsing token type
                if (currentState.Id.TokenType == previousState.Id.TokenType)
                {
                    // This means we successfully found a terminal state for the previously parsing token type
                    // Now check if we have a previous longest token type that is not user defined
                    // This would indicate that we were parsing something built-in (Text, WhiteSpace or Number)
                    // and that we were checking for a valid token. So since we found a valid token, we should
                    // first return the previous built-in token type; then the caller can run the parse again
                    // for the next token.
                    if (previousLongestState is not null && !previousLongestState.Id.TokenType.IsUserDefined)
                    {
                        tokenType = previousLongestState.Id.TokenType;
                        length = previousLongestLexemeLength;
                    }
                    else
                    {
                        // If we are here, it means that we found a valid token and either didn't have any previous
                        // longest token type or the previous longest token type was user defined and was a prefix
                        // to this token (IE. foo and foobar)
                        tokenType = currentState.Id.TokenType;
                        length = characterCount;
                    }
                }
                else if (previousLongestState is not null)
                {
                    // This means that while parsing a potential longer token, the data ended before we fully parsed
                    // the token. So that means we should return the previous longest token (if there is one).
                    tokenType = previousLongestState.Id.TokenType;
                    length = previousLongestLexemeLength;
                }
            }
            else
            {
                Debug.Assert(!currentState.Id.TokenType.IsUserDefined);
                
                // If we have a previous longest state, that could indicate we had partially parsed a prior token
                // Check if it has a default state and use that token type and it's previous length
                // Ex. Defined token 'foo', string "fo "
                if (previousLongestState is not null && previousLongestState.TryGetDefault(out defaultState))
                {
                    Debug.Assert(!defaultState.Id.TokenType.IsUserDefined);
                    tokenType = defaultState.Id.TokenType;
                    length = previousLongestLexemeLength;
                }
                else
                {
                    // Only the built-in types don't have a default (Text, WhiteSpace and Numbers),
                    // So the current state must be a built-in type, and we can just grab the rest
                    // of the data
                    tokenType = currentState.Id.TokenType;
                    length = characterCount;
                }
            }
        }

        if (tokenType is null)
            return false;

        return true;
    }
}