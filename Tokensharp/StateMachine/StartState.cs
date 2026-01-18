using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class StartState<TTokenType>(ITokenTreeNode<TTokenType> rootNode, FrozenDictionary<char, IState<TTokenType>> states, WhiteSpaceState<TTokenType> whiteSpaceState, NumberState<TTokenType> numberState, TextState<TTokenType> textState)
    : NodeStateBase<TTokenType>(rootNode.RootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private WhiteSpaceState<TTokenType> WhiteSpaceStateInstance { get; } = whiteSpaceState;
    private NumberState<TTokenType> NumberStateInstance { get; } = numberState;
    private TextState<TTokenType> TextStateInstance { get; } = textState;

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (states.TryGetValue(c, out nextState))
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
        return new PotentialTokenState<TTokenType>(childNode, fallbackState, null);
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

    public static StartState<TTokenType> For(ITokenTreeNode<TTokenType> tokenTree)
    {
        var whiteSpaceState = new WhiteSpaceState<TTokenType>(tokenTree);
        var numberState = new NumberState<TTokenType>(tokenTree);
        var textState = new TextState<TTokenType>(tokenTree);
        
        Dictionary<char, IState<TTokenType>> startStates = new();
        Dictionary<char, IState<TTokenType>> textWhiteSpaceNumberStates = new();

        foreach (ITokenTreeNode<TTokenType> startNode in tokenTree.RootNode)
        {
            startStates.Add(startNode.Character, PotentialTokenState<TTokenType>.For(startNode, GetFallbackState));
            
            if (startNode.IsEndOfToken)
                textWhiteSpaceNumberStates.Add(startNode.Character, GetFallbackState(startNode).EndOfTokenStateInstance);
            else
                textWhiteSpaceNumberStates.Add(startNode.Character, StartOfCheckForTokenState<TTokenType>.For(startNode, GetFallbackState));
        }

        FrozenDictionary<char, IState<TTokenType>> compiledTextWhiteSpaceNumberStates =
            textWhiteSpaceNumberStates.ToFrozenDictionary();
        
        whiteSpaceState.InitializeStates(compiledTextWhiteSpaceNumberStates);
        numberState.InitializeStates(compiledTextWhiteSpaceNumberStates);
        textState.InitializeStates(compiledTextWhiteSpaceNumberStates);
        
        return new StartState<TTokenType>(tokenTree.RootNode, startStates.ToFrozenDictionary(), whiteSpaceState, numberState, textState);

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