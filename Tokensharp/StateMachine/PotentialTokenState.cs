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
    private readonly EndOfTokenState<TTokenType> _endOfTokenStateInstance = node.IsEndOfToken
        ? new EndOfTokenState<TTokenType>(node.TokenType)
        : fallbackEndOfTokenStateAccessor.EndOfTokenStateInstance;

    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => _endOfTokenStateInstance;

    protected override IState<TTokenType> GetNextState(in char c)
    {
        if (StateLookup.TryGetState(c, out IState<TTokenType>? nextState))
            return nextState;

        if (Node.IsEndOfToken || !fallbackStateCharacterCheck.CharacterIsValidForState(c))
        {
            return _endOfTokenStateInstance;
        }

        if (rootStates.TryGetState(c, out nextState))
            return nextState;
        
        return _endOfTokenStateInstance;
    }

    protected override IState<TTokenType> GetDefaultState()
    {
        return _endOfTokenStateInstance;
    }

    public override void UpdateCounts(ref StateMachineContext context)
    {
        context.PotentialLexemeLength++;
        if (Node.IsEndOfToken)
            context.FallbackLexemeLength = context.PotentialLexemeLength;
    }

    public static PotentialTokenState<TTokenType> For(ITokenTreeNode<TTokenType> node,
        IState<TTokenType> fallbackState,
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
                    new StartOfCheckForTokenState<TTokenType>(startNode, fallbackState, fallbackEndOfTokenStateAccessor,
                        fallbackStateCharacterCheck));
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