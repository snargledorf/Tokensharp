using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class StartState<TTokenType>(
    ITokenTreeNode<TTokenType> rootNode,
    WhiteSpaceState<TTokenType> whiteSpaceState,
    NumberState<TTokenType> numberState,
    TextState<TTokenType> textState)
    : NodeStateBase<TTokenType>(rootNode.RootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private WhiteSpaceState<TTokenType> WhiteSpaceStateInstance { get; } = whiteSpaceState;
    private NumberState<TTokenType> NumberStateInstance { get; } = numberState;
    private TextState<TTokenType> TextStateInstance { get; } = textState;

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

    public override bool CharacterIsValidForState(char c) => true;

    public static StartState<TTokenType> For(ITokenTreeNode<TTokenType> tokenTree)
    {
        var whiteSpaceState = new WhiteSpaceState<TTokenType>(tokenTree);
        var numberState = new NumberState<TTokenType>(tokenTree);
        var textState = new TextState<TTokenType>(tokenTree);

        var startStates = new StateLookupBuilder<TTokenType>();
        var textWhiteSpaceNumberStates = new StateLookupBuilder<TTokenType>();

        foreach (ITokenTreeNode<TTokenType> startNode in tokenTree.RootNode)
        {
            startStates.Add(startNode.Character, PotentialTokenState<TTokenType>.For(startNode, GetFallbackState));

            if (startNode.IsEndOfToken)
            {
                textWhiteSpaceNumberStates.Add(startNode.Character,
                    GetFallbackState(startNode).EndOfTokenStateInstance);}
            else
            {
                textWhiteSpaceNumberStates.Add(startNode.Character,
                    StartOfCheckForTokenState<TTokenType>.For(startNode, GetFallbackState));}
        }

        IStateLookup<TTokenType> compiledTextWhiteSpaceNumberStates = textWhiteSpaceNumberStates.Build();

        whiteSpaceState.StateLookup = compiledTextWhiteSpaceNumberStates;
        numberState.StateLookup = compiledTextWhiteSpaceNumberStates;
        textState.StateLookup = compiledTextWhiteSpaceNumberStates;

        var startState = new StartState<TTokenType>(tokenTree.RootNode, whiteSpaceState,
            numberState, textState)
        {
            StateLookup = startStates.Build()
        };
        
        return startState;

        IEndOfTokenAccessorState<TTokenType> GetFallbackState(ITokenTreeNode<TTokenType> child)
        {
            if (char.IsWhiteSpace(child.Character))
                return whiteSpaceState;

            if (char.IsDigit(child.Character))
                return numberState;

            return textState;
        }
    }
}