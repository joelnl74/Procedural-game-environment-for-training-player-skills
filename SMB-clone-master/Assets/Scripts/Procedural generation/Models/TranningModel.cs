using System.Collections.Generic;
using UnityEngine;

public enum TranningType
{
    None = 0,
    Walking = 1,
    Short_Jump = 2,
    Medium_Jump = 3,
    Enemies = 4,
    Long_Jump = 5,
    Platform = 6,
    BasicsTest = 7,
}

public class TranningModel
{
    private int _totalPlacedObjects;

    public int WalkingSkill = 0;
    public int ShortJumpSkill = 0;
    public int MediumJumpSkill = 0;
    public int LongJumpSkill = 0;
    public int HighJumpSkill = 0;
    public int EnemySkill = 0;

    private List<TranningType> _currentTranningType;
    private SkillsCollectionConfiguration _skillsCollectionConfiguration;

    public List<TranningType> GetCurrentTrannigType()
        => _currentTranningType;

    public void SetPlayerSkillConfiguration(SkillsCollectionConfiguration skillsCollectionConfiguration)
    {
        _skillsCollectionConfiguration = skillsCollectionConfiguration;
    }


    public void SetTranningType(List<TranningType> tranningTypes)
    {
        ResetSkills();

        _currentTranningType = tranningTypes;

        foreach(var tranningType in tranningTypes)
        {
            var skill = _skillsCollectionConfiguration.skillParameters[(int)tranningType];

            if(_totalPlacedObjects > _skillsCollectionConfiguration._maxObjects)
            {
                break;
            }

            switch (tranningType)
            {
                case TranningType.None:
                    break;
                case TranningType.Walking:
                    break;
                case TranningType.Short_Jump:
                    ShortJumpSkill += Random.Range(skill.min, skill.max);
                    _totalPlacedObjects += ShortJumpSkill;
                    break;
                case TranningType.Medium_Jump:
                    MediumJumpSkill += Random.Range(skill.min, skill.max);
                    _totalPlacedObjects += MediumJumpSkill;
                    break;
                case TranningType.Enemies:
                    EnemySkill += Random.Range(skill.min, skill.max);
                    _totalPlacedObjects += EnemySkill;
                    break;
                case TranningType.Long_Jump:
                    LongJumpSkill += Random.Range(skill.min, skill.max);
                    _totalPlacedObjects += LongJumpSkill;
                    break;
                case TranningType.Platform:
                    HighJumpSkill += Random.Range(skill.min, skill.max);
                    _totalPlacedObjects += HighJumpSkill;
                    break;
                default:
                    ShortJumpSkill += Random.Range(skill.min, skill.max);
                    MediumJumpSkill += Random.Range(skill.min, skill.max);

                    LongJumpSkill += Random.Range(skill.min, skill.max);
                    HighJumpSkill += Random.Range(skill.min, skill.max);
                    EnemySkill += Random.Range(skill.min, skill.max);
                    break;
            }
        }
    }

    private void ResetSkills()
    {
        _totalPlacedObjects = 0;

        WalkingSkill = 0;
        ShortJumpSkill = 0;
        MediumJumpSkill = 0;
        LongJumpSkill = 0;
        HighJumpSkill = 0;
        EnemySkill = 0;
    }
}
