using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class StartState<TTokenType>(
    ITokenTreeNode<TTokenType> rootNode,
    WhiteSpace<TTokenType> whiteSpace,
    Number<TTokenType> number,
    Text<TTokenType> text)
    : NodeStateBase<TTokenType>(rootNode.RootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected override bool TryGetNextState(in char c, out IState<TTokenType> nextState)
    {
        if (base.TryGetNextState(c, out nextState))
            return true;

        if (char.IsWhiteSpace(c))
        {
            nextState = whiteSpace;
            return true;
        }

        if (char.IsDigit(c))
        {
            nextState = number;
            return true;
        }

        nextState = text;
        return true;
    }

    protected override bool TryGetDefaultState(out IState<TTokenType> defaultState)
    {
        defaultState = this;
        return false;
    }

    public static StartState<TTokenType> For(ITokenTreeNode<TTokenType> tokenTree)
    {
        var whiteSpaceState = new WhiteSpace<TTokenType>(tokenTree);
        var numberState = new Number<TTokenType>(tokenTree);
        var textState = new Text<TTokenType>(tokenTree);

        var startStates = new StateLookupBuilder<TTokenType>();
        var textWhiteSpaceNumberStates = new StateLookupBuilder<TTokenType>();

        foreach (ITokenTreeNode<TTokenType> startNode in tokenTree.RootNode)
        {
            TextWhiteSpaceNumberBase<TTokenType> fallback = GetFallbackState(startNode);
            
            if (startNode.HasChildren)
            {
                startStates.Add(startNode.Character, PotentialTokenState<TTokenType>.For(startNode, fallback, fallback, fallback));
            }
            else
            {
                startStates.Add(startNode.Character, new EndOfSingleCharacterTokenState<TTokenType>(startNode.TokenType));
            }

            if (startNode.IsEndOfToken)
            {
                textWhiteSpaceNumberStates.Add(startNode.Character,
                    fallback.EndOfTokenStateInstance);
            }
            else
            {
                textWhiteSpaceNumberStates.Add(startNode.Character,
                    StartOfCheckForTokenState<TTokenType>.For(startNode, fallback, fallback, fallback));
            }
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

        TextWhiteSpaceNumberBase<TTokenType> GetFallbackState(ITokenTreeNode<TTokenType> child)
        {
            if (char.IsWhiteSpace(child.Character))
                return whiteSpaceState;

            if (char.IsDigit(child.Character))
                return numberState;

            return textState;
        }
    }
}