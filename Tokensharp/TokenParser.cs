using Tokensharp.StateMachine;

namespace Tokensharp;

public ref struct TokenParser<TTokenType>(ReadOnlySpan<char> buffer,
    bool moreDataAvailable,
    TokenParserState<TTokenType> state)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly ReadOnlySpan<char> _buffer = buffer;
    private readonly StartState<TTokenType> _startState = state.StartState ?? TTokenType.Configuration.StartState;
    
    private int _consumedChars;

    public int CharsConsumed => _consumedChars;
    
    public TokenParserState<TTokenType> CurrentState => new(_startState);
    
    public TokenType<TTokenType> TokenType { get; private set; }
    
    public ReadOnlySpan<char> Lexeme { get; private set; }

    public TokenParser(ReadOnlySpan<char> buffer, bool moreDataAvailable = false) 
        : this(buffer, moreDataAvailable, default(TokenParserState<TTokenType>))
    {
    }

    public TokenParser(ReadOnlySpan<char> buffer, TokenConfiguration<TTokenType> tokenConfiguration)
        : this(buffer, false, tokenConfiguration)
    {
    }

    public TokenParser(ReadOnlySpan<char> buffer, bool moreDataAvailable,
        TokenConfiguration<TTokenType> tokenConfiguration)
        : this(buffer, moreDataAvailable, new TokenParserState<TTokenType>(tokenConfiguration))
    {
    }

    public TokenParser(ReadOnlySpan<char> buffer, TokenParserState<TTokenType> state)
        : this(buffer, false, state)
    {
    }

    public bool Read()
    {
        State<TTokenType> currentState = _startState;
        var context = new StateMachineContext();
        
        TokenType<TTokenType>? tokenType = null;
        int length = 0;

        for (int index = _consumedChars; index < _buffer.Length; index++)
        {
            char c = _buffer[index];
            currentState = currentState.Transition(c, ref context);
            if (currentState.FinalizeToken(ref context, ref tokenType, ref length))
            {
                TokenType = tokenType;
                Lexeme = _buffer[..length];
                _consumedChars += length;
                return true;
            }
        }

        if (!moreDataAvailable)
            currentState = currentState.PerformDefaultTransition(ref context);

        if (currentState.FinalizeToken(ref context, ref tokenType, ref length))
        {
            TokenType = tokenType;
            Lexeme = _buffer[..length];
            _consumedChars += length;
            return true;
        }
        
        return false;
    }
}