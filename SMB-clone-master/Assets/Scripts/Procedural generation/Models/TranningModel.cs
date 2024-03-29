using System.Collections.Generic;
using UnityEngine;

public enum TrainingType
{
    None = 0,
    Walking = 1,
    Short_Jump = 2,
    Medium_Jump = 3,
    Enemies = 4,
    Long_Jump = 5,
    Platform = 6,
    FireBar = 7,
    BasicsTest = 8,
}

public class TranningModel
{
    private int _totalPlacedObjects;
    private const int _maxObjects = 9;

    public int WalkingSkill = 0;
    public int ShortJumpSkill = 0;
    public int MediumJumpSkill = 0;
    public int LongJumpSkill = 0;
    public int HighJumpSkill = 0;
    public int EnemySkill = 0;
    public int FireBarSkill = 0;

    public int Difficulty;

    private SkillsCollectionConfiguration _skillsCollectionConfiguration;

    public void SetPlayerSkillConfiguration(SkillsCollectionConfiguration skillsCollectionConfiguration)
    {
        _skillsCollectionConfiguration = skillsCollectionConfiguration;
    }

    public void SetAdaptiveTranningType(List<TrainingType> items, int difficulty)
    {
        Difficulty = difficulty;

        ResetSkills();

        foreach (var traningSkill in items)
        {
            if(_totalPlacedObjects > _maxObjects)
            {
                break;
            }

            switch (traningSkill)
            {
                case TrainingType.None:
                    break;
                case TrainingType.Walking:
                    break;
                case TrainingType.Short_Jump:
                    ShortJumpSkill += 1;
                    _totalPlacedObjects += ShortJumpSkill;
                    break;
                case TrainingType.Medium_Jump:
                    MediumJumpSkill += 1;
                    break;
                case TrainingType.Enemies:
                    EnemySkill += 1;
                    break;
                case TrainingType.Long_Jump:
                    LongJumpSkill += 1;
                    break;
                case TrainingType.FireBar:
                    FireBarSkill += 1;
                    break;
                case TrainingType.Platform:
                    HighJumpSkill += 1;
                    break;
                case TrainingType.BasicsTest:
                default:
                    break;
            }

            _totalPlacedObjects += traningSkill != TrainingType.Enemies ? 1 : 0;
        }
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
                case TrainingType.None:
                    break;
                case TrainingType.Walking:
                    break;
                case TrainingType.Short_Jump:
                    ShortJumpSkill += Random.Range(traningSkill.min, traningSkill.max);
                    _totalPlacedObjects += ShortJumpSkill;
                    break;
                case TrainingType.Medium_Jump:
                    MediumJumpSkill += Random.Range(traningSkill.min, traningSkill.max);
                    _totalPlacedObjects += MediumJumpSkill;
                    break;
                case TrainingType.Enemies:
                    EnemySkill += Random.Range(traningSkill.min, traningSkill.max);
                    _totalPlacedObjects += EnemySkill;
                    break;
                case TrainingType.Long_Jump:
                    LongJumpSkill += Random.Range(traningSkill.min, traningSkill.max);
                    _totalPlacedObjects += LongJumpSkill;
                    break;
                case TrainingType.FireBar:
                    FireBarSkill += Random.Range(traningSkill.min, traningSkill.max);
                    _totalPlacedObjects += FireBarSkill;
                    break;
                case TrainingType.Platform:
                    HighJumpSkill += Random.Range(traningSkill.min, traningSkill.max);
                    _totalPlacedObjects += HighJumpSkill;
                    break;
                case TrainingType.BasicsTest:
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
