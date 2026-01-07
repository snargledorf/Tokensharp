using System.Collections.Frozen;
using System.Linq.Expressions;

namespace Tokensharp.StateMachine;

public class TokenizerStateBuilder<TTokenType>(TTokenType tokenType) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, TokenizerStateBuilder<TTokenType>> _charToStateBuilder = new();
    private readonly HashSet<ConditionToStateBuilder> _conditionToStateBuilders = [];
    
    private TokenizerStateBuilder<TTokenType>? _defaultTokenStateBuilder;

    private TokenizerState<TTokenType>? _state;

    public TTokenType TokenType { get; } = tokenType;

    public TokenizerStateBuilder<TTokenType> When(char c, TTokenType tokenType)
    {
        var tokenizerStateBuilder = new TokenizerStateBuilder<TTokenType>(tokenType);
        When(c, tokenizerStateBuilder);
        return tokenizerStateBuilder;
    }

    public void When(char c, TokenizerStateBuilder<TTokenType> tokenizerStateBuilder)
    {
        if (_state is not null)
            throw new InvalidOperationException("Builder has already been built");
        
        _charToStateBuilder[c] = tokenizerStateBuilder;
    }

    public TokenizerStateBuilder<TTokenType> When(Expression<Func<char, bool>> condition, TTokenType tokenType)
    {
        var tokenTypeStateBuilder = new TokenizerStateBuilder<TTokenType>(tokenType);
        
        When(condition, tokenTypeStateBuilder);
        
        return tokenTypeStateBuilder;
    }

    public void When(Expression<Func<char, bool>> condition, TokenizerStateBuilder<TTokenType> tokenTypeStateBuilder)
    {
        if (_state is not null)
            throw new InvalidOperationException("Builder has already been built");
        
        _conditionToStateBuilders.Add(new ConditionToStateBuilder(condition, tokenTypeStateBuilder));
    }

    public TokenizerStateBuilder<TTokenType> Default(TTokenType tokenType)
    {
        var defaultTokenStateBuilder = new TokenizerStateBuilder<TTokenType>(tokenType);
        Default(defaultTokenStateBuilder);
        return defaultTokenStateBuilder;
    }

    public void Default(TokenizerStateBuilder<TTokenType> defaultTokenizerStateBuilder)
    {
        if (_state is not null)
            throw new InvalidOperationException("Builder has already been built");
        
        _defaultTokenStateBuilder = defaultTokenizerStateBuilder;
    }

    public TokenizerState<TTokenType> Build()
    {
        if (_state is { } state)
            return state;

        FrozenSet<Transition<TTokenType>> conditionTransitions = _conditionToStateBuilders.Select(csb =>
        {
            Func<char, bool> condition = csb.ConditionExpression.Compile();
            TokenizerState<TTokenType> transitionState = csb.TokenizerStateBuilder.Build();
            return new Transition<TTokenType>(condition, transitionState);
        }).ToFrozenSet();

        FrozenDictionary<char, TokenizerState<TTokenType>> directInputTransitions =
            _charToStateBuilder.ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value.Build());

        TokenizerState<TTokenType>? defaultTransitionState = _defaultTokenStateBuilder?.Build();

        return _state = new TokenizerState<TTokenType>(
            TokenType,
            directInputTransitions, 
            conditionTransitions,
            defaultTransitionState);
    }

    private readonly record struct ConditionToStateBuilder(
        Expression<Func<char, bool>> ConditionExpression,
        TokenizerStateBuilder<TTokenType> TokenizerStateBuilder);
}