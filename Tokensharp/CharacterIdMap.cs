namespace Tokensharp;

public readonly ref struct CharacterIdMap
{
    private readonly int _characterIdOffset;
    private readonly int[] _characterIds;

    public CharacterIdMap(HashSet<char> allCharacters)
    {
        if (allCharacters.Count == 0)
        {
            _characterIdOffset = 0;
            _characterIds = [];
            return;
        }
        
        _characterIdOffset = allCharacters.Min(c => (int)c);
        
        int characterIdsLength = allCharacters.Max(c => (int)c) - _characterIdOffset + 1;
        _characterIds = new int[characterIdsLength];
        Array.Fill(_characterIds, -1);

        int characterId = 0;
        foreach (char c in allCharacters)
        {
            int index = c - _characterIdOffset;
            _characterIds[index] = characterId++;
        }
    }

    public readonly bool TryGetCharacterId(char c, out int characterId)
    {
        int index = c - _characterIdOffset;
        if (index < 0 || index >= _characterIds.Length)
        {
            characterId = -1;
            return false;
        }
        
        characterId = _characterIds[index];
        return characterId != -1;
    }

    public readonly int this[char c]
    {
        get
        {
            if (!TryGetCharacterId(c, out int characterId))
                throw new ArgumentOutOfRangeException(nameof(c));
            return characterId;
        }
    }
}