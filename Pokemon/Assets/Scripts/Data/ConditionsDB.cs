using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{

    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "독에 걸렸다!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}이(가) 독에 의해 고통받고 있다.");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "화상을 입었다!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}은 화상으로 괴로워하고 있다!");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "마비가 됐다!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}은 마비로 몸을 움직일 수 없다!");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "얼어붙었다!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}의 얼음이 녹았다!");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "잠들었다!",
                OnStart = (Pokemon pokemon) =>
                { 
                    // 잠은 1턴에서 3턴 사이의 턴만큼 잠이 들게 된다.
                    pokemon.StatusTime = Random.Range(1, 4);
                    Debug.Log($"{pokemon.StatusTime}턴 동안 잠에 빠진다.");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {

                    if (pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}이(가) 잠에서 깨어났다!");
                        return true;
                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}는 잠들어 있다.");
                    return false;
                }
            }
        },
        
        // Volatile Status Conditions
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "혼수 상태에 빠졌다!",
                OnStart = (Pokemon pokemon) =>
                { 
                    // 혼수 상태는 1턴에서 4턴 사이의 턴만큼 혼수 상태가 된다.
                    pokemon.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"{pokemon.VolatileStatusTime}턴 동안 혼수 상태에 빠진다.");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {

                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}이(가) 혼수 상태에서 깨어났다!");
                        return true;
                    }
                    pokemon.VolatileStatusTime--;

                    // 50% chance to do a move
                    if (Random.Range(1, 3) == 1)
                        return true;
                    
                    // Hurt by confusion
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}는 혼수 상태이다.");
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"혼란에 빠져서 스스로에게 피해를 입었다!");
                    return false;
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
            return 2f;
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn)
            return 1.5f;

        return 1f;
    }
}
public enum ConditionID
{
    none, psn, brn, slp, par, frz,
    confusion
    }