using System.Collections.Generic;
using UnityEngine;

public enum TranningType
{
    None = 0,
    Walking = 1,
    Short_Jump = 2,
    Medium_Jump = 3,
    Enemies = 4,
    Enemies_Jump = 5,
    Long_Jump = 6,
    Enemies_long_jump = 7,
    Platform = 8,
    Enemies_platform = 9,
    Enemies_chasm_platform = 10,
    BasicsTest = 11,
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

    private SkillsCollectionConfiguration _skillsCollectionConfiguration;

    public void SetPlayerSkillConfiguration(SkillsCollectionConfiguration skillsCollectionConfiguration)
    {
        _skillsCollectionConfiguration = skillsCollectionConfiguration;
    }


    public void SetTranningType(int index)
    {
        ResetSkills();

        var skill = _skillsCollectionConfiguration.skillParameters[index];

        foreach (var traningSkill in skill.skillParameters)
        {
            if (_totalPlacedObjects > skill.maxObjects)
            {
                break;
            }

            switch (traningSkill.tranningType)
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
