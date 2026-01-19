using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class PotentialTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IEndOfTokenAccessorState<TTokenType> fallbackState,
    IStateLookup<TTokenType> rootStates)
    : NodeStateBase<TTokenType>(node), IEndOfTokenAccessorState<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public EndOfTokenState<TTokenType> EndOfTokenStateInstance { get; } = node.IsEndOfToken
        ? new EndOfTokenState<TTokenType>(node.TokenType)
        : fallbackState.EndOfTokenStateInstance;

    protected override bool TryGetNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (TryGetStateForChildNode(c, out nextState))
            return true;

        if (Node.IsEndOfToken || !CharacterIsValidForState(c))
        {
            nextState = EndOfTokenStateInstance;
            return true;
        }

        if (rootStates.TryGetState(c, out nextState))
            return true;
        return TryGetDefaultState(out nextState);
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = EndOfTokenStateInstance;
        return true;
    }

    public override bool UpdateCounts(ref int potentialLexemeLength, ref int fallbackLexemeLength,
        ref int confirmedLexemeLength, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType)
    {
        potentialLexemeLength++;
        if (Node.IsEndOfToken)
            fallbackLexemeLength = potentialLexemeLength;
        tokenType = null;
        return false;
    }

    public override bool CharacterIsValidForState(char c) => fallbackState.CharacterIsValidForState(c);

    public static IState<TTokenType> For(ITokenTreeNode<TTokenType> node, IEndOfTokenAccessorState<TTokenType> fallbackState)
    {
        var rootStates = new StateLookupBuilder<TTokenType>();

        foreach (ITokenTreeNode<TTokenType> startNode in node.RootNode)
        {
            if (startNode.IsEndOfToken)
                rootStates.Add(startNode.Character, fallbackState.EndOfTokenStateInstance);
            else
                rootStates.Add(startNode.Character, StartOfCheckForTokenState<TTokenType>.For(startNode, fallbackState));
        }
        
        if (node.IsEndOfToken)
            fallbackState = new EndOfTokenState<TTokenType>(node.TokenType);
        
        var childStates = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            childStates.Add(childNode.Character, For(childNode, fallbackState));
        }
        
        return new PotentialTokenState<TTokenType>(node, fallbackState, rootStates.Build())
        {
            StateLookup = childStates.Build()
        };
    }
}