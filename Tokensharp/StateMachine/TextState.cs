using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class TextState<TTokenType>(ITokenTreeNode<TTokenType> node) : State<TTokenType>()
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public static readonly EndOfTokenState<TTokenType> EndOfTextState = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.Text);
    
    private readonly Dictionary<char, IState<TTokenType>> _states = new();
    
    private WhiteSpaceState<TTokenType>? _whiteSpaceState;
    private NumberState<TTokenType>? _numberState;

    private static TextState<TTokenType>? _instance;

    private WhiteSpaceState<TTokenType> WhiteSpaceState => _whiteSpaceState ?? throw new InvalidOperationException($"{nameof(WhiteSpaceState)} not initialized");
    private NumberState<TTokenType> NumberState => _numberState ?? throw new InvalidOperationException($"{nameof(NumberState)} not initialized");

    internal static TextState<TTokenType> For(ITokenTreeNode<TTokenType> treeNode)
    {
        if (_instance is { } instance)
            return instance;
        
        _instance = new TextState<TTokenType>(treeNode.RootNode);

        _instance.Initialize();

        return _instance;
    }

    private void Initialize()
    {
        _whiteSpaceState = WhiteSpaceState<TTokenType>.For(node);
        _numberState = NumberState<TTokenType>.For(node);
    }

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (char.IsDigit(c) || char.IsWhiteSpace(c))
        {
            nextState = EndOfTextState;
            return true;
        }
        
        if (_states.TryGetValue(c, out nextState))
            return true;
        
        if (node.RootNode.TryGetChild(c, out ITokenTreeNode<TTokenType>? childNode))
        {
            if (childNode.IsEndOfToken)
            {
                nextState = EndOfTokenState<TTokenType>.For(childNode.TokenType);
                return true;
            }
            
            if (!char.IsDigit(c) && !char.IsWhiteSpace(c))
                nextState = new CheckForTextTokenState<TTokenType>(childNode, this, WhiteSpaceState, NumberState);
            else
                nextState = new CheckForMixedTypeTokenState<TTokenType>(childNode, this, this,
                    WhiteSpaceState, NumberState);

            _states.Add(c, nextState);
            return true;
        }
        
        nextState = this;
        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = EndOfTextState;
        return true;
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        context.TokenType = TokenType<TTokenType>.Text;
        base.OnEnter(context);
    }
}