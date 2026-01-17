using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class RootState<TTokenType>(ITokenTreeNode<TTokenType> rootNode)
    : State<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, IState<TTokenType>> _transitions = new();

    protected ITokenTreeNode<TTokenType> RootNode { get; } = rootNode.RootNode;
    internal abstract WhiteSpaceState<TTokenType> WhiteSpaceStateInstance { get; }
    internal abstract NumberState<TTokenType> NumberStateInstance { get; }
    internal abstract TextState<TTokenType> TextStateInstance { get; }

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (_transitions.TryGetValue(c, out nextState)) 
            return true;

        if (RootNode.TryGetChild(c, out ITokenTreeNode<TTokenType>? childNode))
        {
            if (childNode.IsEndOfToken)
            {
                nextState = GetFallbackEndOfTokenState(childNode);
                return true;
            }
            
            nextState = GetStateForChildNode(childNode);

            _transitions.Add(c, nextState);
            return true;
        }

        nextState = null;
        return false;
    }

    protected virtual IState<TTokenType> GetStateForChildNode(ITokenTreeNode<TTokenType> childNode)
    {
        IState<TTokenType> fallbackState = GetFallbackState(childNode);
        return new PotentialTokenState<TTokenType>(childNode, fallbackState, WhiteSpaceStateInstance,
            NumberStateInstance, TextStateInstance);
    }

    private IState<TTokenType> GetFallbackState(ITokenTreeNode<TTokenType> node)
    {
        if (char.IsWhiteSpace(node.Character))
            return WhiteSpaceStateInstance;
        
        if (char.IsDigit(node.Character))
            return NumberStateInstance;
        
        return TextStateInstance;
    }

    protected abstract IState<TTokenType> GetFallbackEndOfTokenState(ITokenTreeNode<TTokenType> node);
}