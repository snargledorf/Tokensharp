using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class StartState<TTokenType>(ITokenTreeNode<TTokenType> rootNode)
    : NodeStateBase<TTokenType>(rootNode.RootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private WhiteSpaceState<TTokenType> WhiteSpaceStateInstance { get; } = new(rootNode);
    private NumberState<TTokenType> NumberStateInstance { get; } = new(rootNode);
    private TextState<TTokenType> TextStateInstance { get; } = new(rootNode);

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (TryGetStateForChildNode(c, out nextState))
            return true;

        if (char.IsWhiteSpace(c))
        {
            nextState = WhiteSpaceStateInstance;
            return true;
        }

        if (char.IsDigit(c))
        {
            nextState = NumberStateInstance;
            return true;
        }

        nextState = TextStateInstance;
        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = null;
        return false;
    }

    protected override IState<TTokenType> CreateStateForChildNode(ITokenTreeNode<TTokenType> childNode)
    {
        IEndOfTokenAccessorState<TTokenType> fallbackState = GetFallbackState(childNode.Character);
        return new PotentialTokenState<TTokenType>(childNode, fallbackState);
    }

    public override bool CharacterIsValidForState(char c) => true;

    private IEndOfTokenAccessorState<TTokenType> GetFallbackState(char c)
    {
        if (char.IsWhiteSpace(c))
            return WhiteSpaceStateInstance;

        if (char.IsDigit(c))
            return NumberStateInstance;

        return TextStateInstance;
    }
}