using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class PotentialTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IEndOfTokenAccessorState<TTokenType> fallbackState,
    WhiteSpaceState<TTokenType> whiteSpaceState,
    NumberState<TTokenType> numberState,
    TextState<TTokenType> textState)
    : NodeStateBase<TTokenType>(node), IEndOfTokenAccessorState<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected WhiteSpaceState<TTokenType> WhiteSpaceStateInstance { get; } = whiteSpaceState;
    protected NumberState<TTokenType> NumberStateInstance { get; } = numberState;
    protected TextState<TTokenType> TextStateInstance { get; } = textState;

    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => field ??= Node.IsEndOfToken
        ? EndOfTokenState<TTokenType>.For(Node.TokenType)
        : fallbackState.EndOfTokenStateInstance;

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (Node.TryGetChild(c, out ITokenTreeNode<TTokenType>? childNode))
        {
            IEndOfTokenAccessorState<TTokenType> childFallback = Node.IsEndOfToken
                ? EndOfTokenState<TTokenType>.For(Node.TokenType)
                : fallbackState;
            
            nextState = new PotentialTokenState<TTokenType>(childNode, childFallback, WhiteSpaceStateInstance,
                NumberStateInstance, TextStateInstance);

            return true;
        }

        if (!CharacterIsValidForState(c) || Node.IsEndOfToken)
        {
            nextState = EndOfTokenStateInstance;
            return true;
        }

        if (Node.RootNode.TryGetChild(c, out childNode))
        {
            nextState = new StartOfCheckForTokenState<TTokenType>(childNode, fallbackState);
            return true;
        }

        return TryGetDefaultState(out nextState);
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = EndOfTokenStateInstance;
        return true;
    }

    public override void OnEnter(StateMachineContext context)
    {
        context.PotentialLexemeLength++;
        if (Node.IsEndOfToken)
            context.FallbackLexemeLength = context.PotentialLexemeLength;
    }

    public override bool CharacterIsValidForState(char c) => fallbackState.CharacterIsValidForState(c);
}