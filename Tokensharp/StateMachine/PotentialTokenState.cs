using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal sealed class PotentialTokenState<TTokenType> : NodeStateBase<TTokenType>, IEndOfTokenStateAccessor<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly EndOfTokenState<TTokenType> _endOfTokenStateInstance;

    private readonly IStateCharacterCheck _fallbackStateCharacterCheck;
    private readonly StateLookup<TTokenType> _rootStates;
    private readonly StateLookup<TTokenType> _childStates;

    public PotentialTokenState(ITokenTreeNode<TTokenType> node,
        State<TTokenType> fallbackState,
        IEndOfTokenStateAccessor<TTokenType> fallbackEndOfTokenStateAccessor,
        IStateCharacterCheck fallbackStateCharacterCheck) : base(node)
    {
        _fallbackStateCharacterCheck = fallbackStateCharacterCheck;
        _endOfTokenStateInstance = node.IsEndOfToken
            ? new EndOfTokenState<TTokenType>(node.TokenType)
            : fallbackEndOfTokenStateAccessor.EndOfTokenStateInstance;

        _rootStates = BuildRootStatesLookup(node, fallbackState, fallbackEndOfTokenStateAccessor, fallbackStateCharacterCheck);

        // If this is an end of token node then we should fallback to this token if we don't find a longer one
        if (node.IsEndOfToken)
        {
            var endOfTokenStateAccessor = new EndOfTokenState<TTokenType>(node.TokenType);
            fallbackState = endOfTokenStateAccessor;
            fallbackEndOfTokenStateAccessor = endOfTokenStateAccessor;
            fallbackStateCharacterCheck = endOfTokenStateAccessor;
        }

        _childStates = BuildChildStatesLookup(node, fallbackState, fallbackEndOfTokenStateAccessor, fallbackStateCharacterCheck);
    }

    private static StateLookup<TTokenType> BuildRootStatesLookup(ITokenTreeNode<TTokenType> node, State<TTokenType> fallbackState,
        IEndOfTokenStateAccessor<TTokenType> fallbackEndOfTokenStateAccessor, IStateCharacterCheck fallbackStateCharacterCheck)
    {
        var rootStatesBuilder = new StateLookupBuilder<TTokenType>();

        foreach (ITokenTreeNode<TTokenType> startNode in node.RootNode)
        {
            if (startNode.IsEndOfToken)
            {
                rootStatesBuilder.Add(startNode.Character, fallbackEndOfTokenStateAccessor.EndOfTokenStateInstance);
            }
            else
            {
                rootStatesBuilder.Add(startNode.Character,
                    new StartOfCheckForTokenState<TTokenType>(startNode, fallbackState, fallbackEndOfTokenStateAccessor,
                        fallbackStateCharacterCheck));
            }
        }

        return rootStatesBuilder.Build();
    }

    private static StateLookup<TTokenType> BuildChildStatesLookup(ITokenTreeNode<TTokenType> node, State<TTokenType> fallbackState,
        IEndOfTokenStateAccessor<TTokenType> fallbackEndOfTokenStateAccessor, IStateCharacterCheck fallbackStateCharacterCheck)
    {
        var childStatesBuilder = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            if (childNode.HasChildren)
            {
                childStatesBuilder.Add(childNode.Character,
                    new PotentialTokenState<TTokenType>(childNode, fallbackState, fallbackEndOfTokenStateAccessor,
                        fallbackStateCharacterCheck));
            }
            else
            {
                childStatesBuilder.Add(childNode.Character,
                    new EndOfPotentialTokenState<TTokenType>(childNode.TokenType));
            }
        }

        return childStatesBuilder.Build();
    }

    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => _endOfTokenStateInstance;

    protected override State<TTokenType> GetNextState(char c)
    {
        if (_childStates.TryGetState(c, out State<TTokenType>? nextState) ||
            !Node.IsEndOfToken && _fallbackStateCharacterCheck.CharacterIsValidForState(c) &&
            _rootStates.TryGetState(c, out nextState))
            return nextState;

        return _endOfTokenStateInstance;
    }

    protected override State<TTokenType> DefaultState => _endOfTokenStateInstance;

    public override void UpdateCounts(ref StateMachineContext context)
    {
        int newPotentialLexemeLength = context.PotentialLexemeLength + 1;
        
        context = Node.IsEndOfToken
            ? new StateMachineContext(newPotentialLexemeLength, newPotentialLexemeLength)
            : new StateMachineContext(newPotentialLexemeLength, context.FallbackLexemeLength);
    }
}