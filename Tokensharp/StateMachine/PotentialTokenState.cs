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
    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => field ??= Node.IsEndOfToken
        ? EndOfTokenState<TTokenType>.For(Node.TokenType)
        : fallbackState.EndOfTokenStateInstance;

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (TryGetStateForChildNode(c, out nextState))
            return true;

        if (!CharacterIsValidForState(c) || Node.IsEndOfToken)
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

    public override void OnEnter(StateMachineContext context)
    {
        context.PotentialLexemeLength++;
        if (Node.IsEndOfToken)
            context.FallbackLexemeLength = context.PotentialLexemeLength;
    }

    public override bool CharacterIsValidForState(char c) => fallbackState.CharacterIsValidForState(c);

    public static IState<TTokenType> For(ITokenTreeNode<TTokenType> node, Func<ITokenTreeNode<TTokenType>, IEndOfTokenAccessorState<TTokenType>> getFallbackState)
    {
        var rootStates = new StateLookupBuilder<TTokenType>();

        foreach (ITokenTreeNode<TTokenType> startNode in node.RootNode)
        {
            if (startNode.IsEndOfToken)
                rootStates.Add(startNode.Character, getFallbackState(startNode).EndOfTokenStateInstance);
            else
                rootStates.Add(startNode.Character, StartOfCheckForTokenState<TTokenType>.For(startNode, getFallbackState));
        }
        
        IEndOfTokenAccessorState<TTokenType> fallbackState = getFallbackState(node);
        if (node.IsEndOfToken)
            fallbackState = EndOfTokenState<TTokenType>.For(node.TokenType);
        
        var childStates = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
            childStates.Add(childNode.Character, For(childNode, _ => fallbackState));
        
        return new PotentialTokenState<TTokenType>(node, fallbackState, rootStates.Build())
        {
            StateLookup = childStates.Build()
        };
    }
}