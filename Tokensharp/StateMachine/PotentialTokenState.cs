using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class PotentialTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IEndOfTokenStateAccessor<TTokenType> fallbackEndOfTokenStateAccessor,
    IStateCharacterCheck fallbackStateCharacterCheck,
    IStateLookup<TTokenType> rootStates)
    : NodeStateBase<TTokenType>(node), IEndOfTokenStateAccessor<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public EndOfTokenState<TTokenType> EndOfTokenStateInstance { get; } = node.IsEndOfToken
        ? new EndOfTokenState<TTokenType>(node.TokenType)
        : fallbackEndOfTokenStateAccessor.EndOfTokenStateInstance;

    protected override bool TryGetNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (TryGetStateForChildNode(c, out nextState))
            return true;

        if (Node.IsEndOfToken || !fallbackStateCharacterCheck.CharacterIsValidForState(c))
        {
            nextState = EndOfTokenStateInstance;
            return true;
        }

        return rootStates.TryGetState(c, out nextState) || TryGetDefaultState(out nextState);
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = EndOfTokenStateInstance;
        return true;
    }

    public override void UpdateCounts(ref StateMachineContext context)
    {
        context.PotentialLexemeLength++;
        if (Node.IsEndOfToken)
            context.FallbackLexemeLength = context.PotentialLexemeLength;
    }

    public static PotentialTokenState<TTokenType> For(ITokenTreeNode<TTokenType> node,
        State<TTokenType> fallbackState,
        IEndOfTokenStateAccessor<TTokenType> fallbackEndOfTokenStateAccessor,
        IStateCharacterCheck fallbackStateCharacterCheck)
    {
        var rootStates = new StateLookupBuilder<TTokenType>();

        foreach (ITokenTreeNode<TTokenType> startNode in node.RootNode)
        {
            if (startNode.IsEndOfToken)
                rootStates.Add(startNode.Character, fallbackEndOfTokenStateAccessor.EndOfTokenStateInstance);
            else
                rootStates.Add(startNode.Character,
                    StartOfCheckForTokenState<TTokenType>.For(startNode, fallbackState, fallbackEndOfTokenStateAccessor, fallbackStateCharacterCheck));
        }

        if (node.IsEndOfToken)
        {
            var endOfTokenStateAccessor = new EndOfTokenState<TTokenType>(node.TokenType);
            fallbackState = endOfTokenStateAccessor;
            fallbackEndOfTokenStateAccessor = endOfTokenStateAccessor;
            fallbackStateCharacterCheck = endOfTokenStateAccessor;
        }

        var childStates = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            if (childNode.HasChildren)
                childStates.Add(childNode.Character,
                    For(childNode, fallbackState, fallbackEndOfTokenStateAccessor, fallbackStateCharacterCheck));
            else
                childStates.Add(childNode.Character, new EndOfPotentialTokenState<TTokenType>(childNode.TokenType));
        }

        return new PotentialTokenState<TTokenType>(node, fallbackEndOfTokenStateAccessor, fallbackStateCharacterCheck,
            rootStates.Build())
        {
            StateLookup = childStates.Build()
        };
    }
}