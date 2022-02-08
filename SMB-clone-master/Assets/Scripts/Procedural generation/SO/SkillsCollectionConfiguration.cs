using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class TraningSkillClass
{
    public TranningType TranningType;
    public int maxObjects;

    public List<SkillParameters> skillParameters;
}

[CreateAssetMenu(fileName = "Player skill collection", menuName = "Data/Player Skill Collection", order = 1)]
public class SkillsCollectionConfiguration : ScriptableObject
{
    public List<TraningSkillClass> skillParameters;
}
