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

            foreach(var traningSkill in skill.skillParameters)
            {
                if (_totalPlacedObjects > skill.maxObjects)
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
                        ShortJumpSkill += Random.Range(traningSkill.min, traningSkill.max);
                        _totalPlacedObjects += ShortJumpSkill;
                        break;
                    case TranningType.Medium_Jump:
                        MediumJumpSkill += Random.Range(traningSkill.min, traningSkill.max);
                        _totalPlacedObjects += MediumJumpSkill;
                        break;
                    case TranningType.Enemies:
                        EnemySkill += Random.Range(traningSkill.min, traningSkill.max);
                        _totalPlacedObjects += EnemySkill;
                        break;
                    case TranningType.Long_Jump:
                        LongJumpSkill += Random.Range(traningSkill.min, traningSkill.max);
                        _totalPlacedObjects += LongJumpSkill;
                        break;
                    case TranningType.Platform:
                        HighJumpSkill += Random.Range(traningSkill.min, traningSkill.max);
                        _totalPlacedObjects += HighJumpSkill;
                        break;
                    case TranningType.BasicsTest:
                    default:
                        ShortJumpSkill += Random.Range(traningSkill.min, traningSkill.max);
                        MediumJumpSkill += Random.Range(traningSkill.min, traningSkill.max);

                        LongJumpSkill += Random.Range(traningSkill.min, traningSkill.max);
                        HighJumpSkill += Random.Range(traningSkill.min, traningSkill.max);
                        EnemySkill += Random.Range(traningSkill.min, traningSkill.max);
                        break;
                }
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
