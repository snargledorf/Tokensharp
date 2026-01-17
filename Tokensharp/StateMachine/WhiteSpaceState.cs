using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class WhiteSpaceState<TTokenType>(ITokenTreeNode<TTokenType> node) 
    : State<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public static readonly EndOfTokenState<TTokenType> EndOfWhiteSpaceState = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.WhiteSpace);
    
    private readonly Dictionary<char, IState<TTokenType>> _states = new();
    
    private TextState<TTokenType>? _textState;
    private NumberState<TTokenType>? _numberState;

    private static WhiteSpaceState<TTokenType>? _instance;

    private TextState<TTokenType> TextState => _textState ?? throw new InvalidOperationException($"{nameof(TextState)} not initialized");
    private NumberState<TTokenType> NumberState => _numberState ?? throw new InvalidOperationException($"{nameof(NumberState)} not initialized");

    internal static WhiteSpaceState<TTokenType> For(ITokenTreeNode<TTokenType> treeNode)
    {
        if (_instance is { } instance)
            return instance;
        
        _instance = new WhiteSpaceState<TTokenType>(treeNode.RootNode);

        _instance.Initialize();

        return _instance;
    }

    private void Initialize()
    {
        _textState = TextState<TTokenType>.For(node);
        _numberState = NumberState<TTokenType>.For(node);
    }
    
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!char.IsWhiteSpace(c))
        {
            nextState = EndOfWhiteSpaceState;
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
            
            if (char.IsWhiteSpace(c))
                nextState = new CheckForWhiteSpaceTokenState<TTokenType>(childNode, this, NumberState, TextState);
            else
                nextState = new CheckForMixedTypeTokenState<TTokenType>(childNode, this, TextState,
                    this, NumberState);

            _states.Add(c, nextState);
            return true;
        }
        
        nextState = this;
        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = EndOfWhiteSpaceState;
        return true;
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        context.TokenType = TokenType<TTokenType>.WhiteSpace;
        base.OnEnter(context);
    }
}