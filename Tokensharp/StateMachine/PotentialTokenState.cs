using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class PotentialTokenState<TTokenType> : NodeStateBase<TTokenType>, IEndOfTokenStateAccessor<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly EndOfTokenState<TTokenType> _endOfTokenStateInstance;

    private readonly IStateCharacterCheck _fallbackStateCharacterCheck;
    private readonly IStateLookup<TTokenType> _rootStates;

    public PotentialTokenState(ITokenTreeNode<TTokenType> node,
        IState<TTokenType> fallbackState,
        IEndOfTokenStateAccessor<TTokenType> fallbackEndOfTokenStateAccessor,
        IStateCharacterCheck fallbackStateCharacterCheck) : base(node)
    {
        _fallbackStateCharacterCheck = fallbackStateCharacterCheck;
        _endOfTokenStateInstance = node.IsEndOfToken
            ? new EndOfTokenState<TTokenType>(node.TokenType)
            : fallbackEndOfTokenStateAccessor.EndOfTokenStateInstance;
        
        var rootStatesBuilder = new StateLookupBuilder<TTokenType>();

        foreach (ITokenTreeNode<TTokenType> startNode in node.RootNode)
        {
            if (startNode.IsEndOfToken)
                rootStatesBuilder.Add(startNode.Character, fallbackEndOfTokenStateAccessor.EndOfTokenStateInstance);
            else
                rootStatesBuilder.Add(startNode.Character,
                    new StartOfCheckForTokenState<TTokenType>(startNode, fallbackState, fallbackEndOfTokenStateAccessor,
                        fallbackStateCharacterCheck));
        }
        
        _rootStates = rootStatesBuilder.Build();

        // If this is an end of token node then we should fallback to this token if we don't find a longer one
        if (node.IsEndOfToken)
        {
            var endOfTokenStateAccessor = new EndOfTokenState<TTokenType>(node.TokenType);
            fallbackState = endOfTokenStateAccessor;
            fallbackEndOfTokenStateAccessor = endOfTokenStateAccessor;
            fallbackStateCharacterCheck = endOfTokenStateAccessor;
        }

        var childStatesBuilder = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            if (childNode.HasChildren)
                childStatesBuilder.Add(childNode.Character,
                    new PotentialTokenState<TTokenType>(childNode, fallbackState, fallbackEndOfTokenStateAccessor, fallbackStateCharacterCheck));
            else
                childStatesBuilder.Add(childNode.Character, new EndOfPotentialTokenState<TTokenType>(childNode.TokenType));
        }
        
        StateLookup = childStatesBuilder.Build();
    }

    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => _endOfTokenStateInstance;

    protected override IState<TTokenType> GetNextState(in char c)
    {
        if (StateLookup.TryGetState(c, out IState<TTokenType>? nextState) ||
            !Node.IsEndOfToken && _fallbackStateCharacterCheck.CharacterIsValidForState(in c) &&
            _rootStates.TryGetState(c, out nextState))
            return nextState;

        return _endOfTokenStateInstance;
    }

    protected override IState<TTokenType> DefaultState => _endOfTokenStateInstance;

    public override void UpdateCounts(ref StateMachineContext context)
    {
        context.PotentialLexemeLength++;
        if (Node.IsEndOfToken)
            context.FallbackLexemeLength = context.PotentialLexemeLength;
    }
}