using System.Linq.Expressions;

namespace Tokensharp.StateMachine;

public class TokenizerStateBuilder<TTokenType>(TTokenType tokenType) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static readonly Expression<Func<char, bool>> DefaultCondition = _ => true;
    
    private readonly List<ConditionToStateBuilder<TTokenType>> _conditionToStateBuilders = [];
    
    private TokenizerStateBuilder<TTokenType>? _defaultTokenStateBuilder;

    public TTokenType TokenType { get; } = tokenType;

    public TokenizerStateBuilder<TTokenType> When(char c, TTokenType tokenType)
    {
        Expression<Func<char, bool>> condition = InputEqualsExpression(c);
        return When(condition, tokenType);
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

        IEnumerable<ConditionExpressionToState<TTokenType>> conditionExpressionToStates = _conditionToStateBuilders
            .Select(cdb => cdb.Build());

        if (_defaultTokenStateBuilder is { } defaultTokenStateBuilder)
        {
            conditionExpressionToStates = conditionExpressionToStates.Append(
                new ConditionExpressionToState<TTokenType>(DefaultCondition, defaultTokenStateBuilder.Build()));
        }

        foreach (ConditionExpressionToState<TTokenType> conditionExpressionToState in conditionExpressionToStates)
        {
            Func<char, bool> condition = conditionExpressionToState.ConditionExpression.Compile();
            transitions.Add(new Transition<TTokenType>(condition, conditionExpressionToState.State));
        }
        
        return new TokenizerState<TTokenType>(TokenType, transitions);
    }

    private static Expression<Func<char, bool>> InputEqualsExpression(char c)
    {
        return (input) => input.Equals(c);
    }
}

internal record struct ConditionExpressionToState<TTokenType>(
    Expression<Func<char, bool>> ConditionExpression,
    TokenizerState<TTokenType> State) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>;

internal record struct ConditionToStateBuilder<TTokenType>(
    Expression<Func<char, bool>> Condition,
    TokenizerStateBuilder<TTokenType> TokenizerStateBuilder)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public ConditionExpressionToState<TTokenType> Build() => new(Condition, TokenizerStateBuilder.Build());
}