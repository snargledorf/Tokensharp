using Tokensharp.TokenTree;

namespace Tokensharp;

public sealed class TokenConfiguration<TTokenType> : ITokenConfiguration<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    internal TokenConfiguration(IEnumerable<LexemeToTokenType<TTokenType>> lexemeToTokenTypes, bool numbersAreText = false)
    {
        TokenTree = lexemeToTokenTypes.ToTokenTree();
        
        NumbersAreText = numbersAreText;
        
        HashSet<char> allCharacters = TokenTree.AllCharacters;

        CharacterIdMap = new CharacterIdMap(allCharacters);

        var stateTransitions = new List<int[]>();
        var stateToTokenType = new List<TTokenType?>();
        var hasPossibleTransitions = new List<bool>();
        
        BuildTransitions(stateTransitions, stateToTokenType, hasPossibleTransitions, TokenTree);

        StateTransitions = [..stateTransitions];
        StateToTokenType = [..stateToTokenType];
        HasPossibleTransitions = [..hasPossibleTransitions];

        IsEndOfToken = new bool[StateToTokenType.Length];
        for(var i = 0; i < StateToTokenType.Length; i++)
        {
            if (StateToTokenType[i] is not null)
            {
                IsEndOfToken[i] = true;
            }
        }
    }

    public bool NumbersAreText { get; }
    
    public static implicit operator TokenConfiguration<TTokenType>(LexemeToTokenType<TTokenType>[] lexemeToTokenTypes)
    {
        return new TokenConfiguration<TTokenType>(lexemeToTokenTypes);
    }
    
    internal ITokenTreeNode<TTokenType> TokenTree { get; }
    
    internal CharacterIdMap CharacterIdMap { get; }

    internal int[][] StateTransitions { get; }

    internal TTokenType?[] StateToTokenType { get; }

    internal bool[] IsEndOfToken { get; }

    internal bool[] HasPossibleTransitions { get; }

    private void BuildTransitions(List<int[]> stateTransitions, List<TTokenType?> stateToTokenType, List<bool> hasPossibleTransitions, ITokenTreeNode<TTokenType> node)
    {
        stateToTokenType.Add(node.TokenType);
        hasPossibleTransitions.Add(node.HasChildren);
        
        int maxCharacterId = GetMaxChildCharacterId(node);

        var transitions = new int[maxCharacterId + 1];
        Array.Fill(transitions, -1);
        stateTransitions.Add(transitions);

        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            int charId = CharacterIdMap[childNode.Character];
            transitions[charId] = stateTransitions.Count;
            BuildTransitions(stateTransitions, stateToTokenType, hasPossibleTransitions, childNode);
        }
    }

    private int GetMaxChildCharacterId(ITokenTreeNode<TTokenType> node)
    {
        int maxCharacterId = -1;
        
        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            int characterId = CharacterIdMap[childNode.Character];
            if (characterId > maxCharacterId)
            {
                maxCharacterId = characterId;
            }
        }

        return maxCharacterId;
    }
}