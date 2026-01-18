using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class CheckForTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IEndOfTokenAccessorState<TTokenType> fallbackState)
    : NodeStateBase<TTokenType>(node), IEndOfTokenAccessorState<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private IEndOfTokenAccessorState<TTokenType> FallbackState { get; } = fallbackState;

    private FoundTokenState<TTokenType> FoundTokenState =>
        field ??= new FoundTokenState<TTokenType>(FallbackState.EndOfTokenStateInstance);

    private FailedTokenCheckState<TTokenType> FallbackFailedTokenCheckState =>
        field ??= new FailedTokenCheckState<TTokenType>(FallbackState);

    private MixedCharacterFailedTokenCheckState<TTokenType> EndOfFallbackFailedTokenCheckState => field ??=
        new MixedCharacterFailedTokenCheckState<TTokenType>(FallbackState.EndOfTokenStateInstance);

    private MixedCharacterFailedTokenCheckState<TTokenType> DefaultEndOfFallbackFailedTokenCheckState => field ??=
        new MixedCharacterFailedTokenCheckState<TTokenType>(FallbackState.EndOfTokenStateInstance);

    private MixedCharacterFailedTokenCheckState<TTokenType> DefaultFailedTokenCheckState => field ??=
        new MixedCharacterFailedTokenCheckState<TTokenType>(FallbackState);

    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => field ??= FallbackState.EndOfTokenStateInstance;

    public override bool CharacterIsValidForState(char c) => FallbackState.CharacterIsValidForState(c);

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (Node.IsEndOfToken)
        {
            nextState = FoundTokenState;
        }
        else if (!TryGetStateForChildNode(c, out nextState))
        {
            if (CharacterIsValidForState(c))
            {
                nextState = FallbackFailedTokenCheckState;
            }
            else
            {
                nextState = EndOfFallbackFailedTokenCheckState;
            }
        }

        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        if (Node.IsEndOfToken)
        {
            defaultState = FoundTokenState;
        }
        else if (CharacterIsValidForState(Node.Character))
        {
            defaultState = DefaultFailedTokenCheckState;
        }
        else
        {
            defaultState = DefaultEndOfFallbackFailedTokenCheckState;
        }

        return true;
    }

    public override void OnEnter(StateMachineContext context)
    {
        context.PotentialLexemeLength++;
    }

    protected static CheckForTokenState<TTokenType> For(ITokenTreeNode<TTokenType> node,
        Func<ITokenTreeNode<TTokenType>, IEndOfTokenAccessorState<TTokenType>> getFallbackState)
    {
        IEndOfTokenAccessorState<TTokenType> fallbackState = getFallbackState(node);
        
        var childStates = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
            childStates.Add(childNode.Character, For(childNode, getFallbackState));
        
        return new CheckForTokenState<TTokenType>(node, fallbackState)
        {
            StateLookup = childStates.Build()
        };
    }
}