using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class CheckForTextTokenState<TTokenType>(ITokenTreeNode<TTokenType> node, TextState<TTokenType> textState, WhiteSpaceState<TTokenType> whiteSpaceState, NumberState<TTokenType> numberState)
    : CheckForTokenState<TTokenType>(node, textState, whiteSpaceState, numberState, textState) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, IState<TTokenType>> _transitions = new();
    
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (_transitions.TryGetValue(c, out nextState)) 
            return true;
        
        if (Node.TryGetChild(c, out ITokenTreeNode<TTokenType>? nextTreeNode))
        {
            if (nextTreeNode.IsEndOfToken)
            {
                nextState = TextState<TTokenType>.EndOfTextState;
            }
            else
            {
                if (!char.IsWhiteSpace(c) && !char.IsDigit(c))
                {
                    nextState = new CheckForTextTokenState<TTokenType>(nextTreeNode, TextState, WhiteSpaceState,
                        NumberState);
                }
                else
                {
                    nextState = new CheckForMixedTypeTokenState<TTokenType>(nextTreeNode, TextState, TextState,
                        WhiteSpaceState,
                        NumberState);
                }
            }
            
            _transitions.Add(c, nextState);
            return true;
        }
        
        if (char.IsWhiteSpace(c) || char.IsDigit(c))
        {
            nextState = TextState;
            return true;
        }
        
        return TryGetDefaultState(out nextState);
    }
}

internal class CheckForNumberTokenState<TTokenType>(ITokenTreeNode<TTokenType> node, NumberState<TTokenType> numberState, WhiteSpaceState<TTokenType> whiteSpaceState, TextState<TTokenType> textState)
    : CheckForTokenState<TTokenType>(node, numberState, whiteSpaceState, numberState, textState) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, IState<TTokenType>> _transitions = new();
    
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (_transitions.TryGetValue(c, out nextState)) 
            return true;
        
        if (Node.TryGetChild(c, out ITokenTreeNode<TTokenType>? nextTreeNode))
        {
            if (nextTreeNode.IsEndOfToken)
            {
                nextState = NumberState<TTokenType>.EndOfNumberState;
            }
            else
            {
                if (char.IsDigit(c))
                {
                    nextState = new CheckForNumberTokenState<TTokenType>(nextTreeNode, NumberState, WhiteSpaceState,
                        TextState);
                }
                else
                {
                    nextState = new CheckForMixedTypeTokenState<TTokenType>(nextTreeNode, NumberState, TextState,
                        WhiteSpaceState,
                        NumberState);
                }
            }

            _transitions.Add(c, nextState);
            return true;
        }
        
        if (!char.IsDigit(c))
        {
            nextState = NumberState<TTokenType>.EndOfNumberState;
            return true;
        }
        
        return TryGetDefaultState(out nextState);
    }
}

internal class CheckForWhiteSpaceTokenState<TTokenType>(ITokenTreeNode<TTokenType> node, WhiteSpaceState<TTokenType> whiteSpaceState, NumberState<TTokenType> numberState, TextState<TTokenType> textState)
    : CheckForTokenState<TTokenType>(node, whiteSpaceState, whiteSpaceState, numberState, textState) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, IState<TTokenType>> _transitions = new();
    
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (_transitions.TryGetValue(c, out nextState)) 
            return true;
        
        if (Node.TryGetChild(c, out ITokenTreeNode<TTokenType>? nextTreeNode))
        {
            if (nextTreeNode.IsEndOfToken)
            {
                nextState = WhiteSpaceState<TTokenType>.EndOfWhiteSpaceState;
            }
            else
            {
                if (char.IsWhiteSpace(c))
                {
                    nextState = new CheckForWhiteSpaceTokenState<TTokenType>(nextTreeNode, WhiteSpaceState,
                        NumberState, TextState);
                }
                else
                {
                    nextState = new CheckForMixedTypeTokenState<TTokenType>(nextTreeNode, WhiteSpaceState, TextState,
                        WhiteSpaceState, NumberState);
                }
            }
            
            _transitions.Add(c, nextState);
            return true;
        }
        
        if (!char.IsWhiteSpace(c))
        {
            nextState = WhiteSpaceState<TTokenType>.EndOfWhiteSpaceState;
            return true;
        }
        
        return TryGetDefaultState(out nextState);
    }
}