using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player skill collection", menuName = "Data/Player Skill Collection", order = 1)]
public class SkillsCollectionConfiguration : ScriptableObject
{
    public int _maxObjects;
    public int _minObjects;

    public List<SkillParameters> skillParameters;
}
