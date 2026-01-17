using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class CheckForTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IState<TTokenType> defaultState,
    WhiteSpaceState<TTokenType> whiteSpaceState,
    NumberState<TTokenType> numberState,
    TextState<TTokenType> textState)
    : BaseState<TTokenType>(node, whiteSpaceState, numberState, textState)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly IState<TTokenType> _defaultState = defaultState;

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = _defaultState;
        return true;
    }
    
    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        if (context.PotentialLexemeLength == 0)
            context.PotentialLexemeLength = context.ConfirmedLexemeLength;
        
        context.PotentialLexemeLength++;
    }
}