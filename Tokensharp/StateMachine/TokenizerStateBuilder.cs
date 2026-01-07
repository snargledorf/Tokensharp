using System.Collections.Frozen;
using System.Linq.Expressions;

namespace Tokensharp.StateMachine;

public class TokenizerStateBuilder<TTokenType>(TTokenType tokenType) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, TokenizerStateBuilder<TTokenType>> _charToStateBuilder = new();
    private readonly List<ConditionToStateBuilder<TTokenType>> _conditionToStateBuilders = [];
    
    private TokenizerStateBuilder<TTokenType>? _defaultTokenStateBuilder;

    public TTokenType TokenType { get; } = tokenType;

    public TokenizerStateBuilder<TTokenType> When(char c, TTokenType tokenType)
    {
        return _charToStateBuilder[c] = new TokenizerStateBuilder<TTokenType>(tokenType);
    }

    public TokenizerStateBuilder<TTokenType> When(Expression<Func<char, bool>> condition, TTokenType tokenType)
    {
        var tokenTypeStateBuilder = new TokenizerStateBuilder<TTokenType>(tokenType);
        
        _conditionToStateBuilders.Add(new ConditionToStateBuilder<TTokenType>(condition, tokenTypeStateBuilder));
        
        return tokenTypeStateBuilder;
    }

    public TokenizerStateBuilder<TTokenType> Default(TTokenType tokenType)
    {
        return _defaultTokenStateBuilder = new TokenizerStateBuilder<TTokenType>(tokenType);
    }

    public TokenizerState<TTokenType> Build()
    {
        var transitions = new List<Transition<TTokenType>>();

        foreach (ConditionToStateBuilder<TTokenType> conditionExpressionToState in _conditionToStateBuilders)
        {
            Func<char, bool> condition = conditionExpressionToState.ConditionExpression.Compile();
            TokenizerState<TTokenType> state = conditionExpressionToState.TokenizerStateBuilder.Build();
            transitions.Add(new Transition<TTokenType>(condition, state));
        }

        return new TokenizerState<TTokenType>(TokenType,
            _charToStateBuilder.ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value.Build()), transitions,
            _defaultTokenStateBuilder?.Build());
    }
}

internal readonly record struct ConditionToStateBuilder<TTokenType>(
    Expression<Func<char, bool>> ConditionExpression,
    TokenizerStateBuilder<TTokenType> TokenizerStateBuilder)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>;