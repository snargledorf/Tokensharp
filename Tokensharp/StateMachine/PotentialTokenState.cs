using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class PotentialTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IEndOfTokenAccessorState<TTokenType> fallbackState,
    FrozenDictionary<char, IState<TTokenType>> rootStates)
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

        if (rootStates.TryGetValue(c, out nextState))
            return true;

        return TryGetDefaultState(out nextState);
    }

    protected override IState<TTokenType> CreateStateForChildNode(ITokenTreeNode<TTokenType> childNode)
    {
        IEndOfTokenAccessorState<TTokenType> childFallback = Node.IsEndOfToken
            ? EndOfTokenState<TTokenType>.For(Node.TokenType)
            : fallbackState;
            
        return new PotentialTokenState<TTokenType>(childNode, childFallback, rootStates);
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
        IEndOfTokenAccessorState<TTokenType> fallbackState = getFallbackState(node);
        if (node.IsEndOfToken)
            fallbackState = EndOfTokenState<TTokenType>.For(node.TokenType);

        Dictionary<char, IState<TTokenType>> rootStates = new();

        foreach (ITokenTreeNode<TTokenType> startNode in node.RootNode)
        {
            fallbackState = getFallbackState(startNode);
            if (startNode.IsEndOfToken)
                rootStates.Add(startNode.Character, fallbackState.EndOfTokenStateInstance);
            else
                rootStates.Add(startNode.Character, new StartOfCheckForTokenState<TTokenType>(startNode, fallbackState));
        }
        
        return new PotentialTokenState<TTokenType>(node, fallbackState, rootStates.ToFrozenDictionary());
    }
}