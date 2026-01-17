using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class PotentialTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IState<TTokenType> defaultState,
    WhiteSpaceState<TTokenType> whiteSpaceState,
    NumberState<TTokenType> numberState,
    TextState<TTokenType> textState)
    : BaseState<TTokenType>(node, whiteSpaceState, numberState, textState)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly IState<TTokenType> _defaultState = defaultState;
    
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (base.TryGetStateNextState(c, out nextState))
            return true;

        return TryGetDefaultState(out nextState);
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = _defaultState;
        return true;
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        if (Node.IsEndOfToken)
        {
            context.TokenType = Node.TokenType;
            context.PotentialLexemeLength++;
            context.ConfirmedLexemeLength = context.PotentialLexemeLength;
        }
        else
        {
            context.PotentialLexemeLength++;
        }
    }
}