using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

    public PokemonBase Base
    {
        get
        {
            return _base;
        }
    }
    public int Level
    {
        get
        {
            return level;
        }
    }

    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }

    public Queue<string> StatusChanges { get; private set; }
    public event System.Action OnStatusChanged;
    public event System.Action OnHPChanged;

    public void Init()
    {
        // Generate Moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= PokemonBase.MaxNumOfMoves)
                break;
        }

        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;

        StatusChanges = new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetObjectByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusId != null)
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        else
            Status = null;

        Moves = saveData.moves.Select(s => new Move(s)).ToList();

        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        VolatileStatus = null;
    }

    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.Name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };
        return saveData;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0},
        };
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        int oldMaxHP = MaxHp;
        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level;

        if (oldMaxHP !=0)
            HP += MaxHp - oldMaxHP;
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);


        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            string statString = GetKoreaStat(statBoost);
            if (boost > 0)
                StatusChanges.Enqueue($"{Base.Name}의 {statString}이(가) 올랐다!");

            else
                StatusChanges.Enqueue($"{Base.Name}의 {statString}이(가) 떨어졌다!");

            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    private string GetKoreaStat(StatBoost statBoost)
    {
        if (statBoost.stat == Stat.Attack)
            return "공격력";
        else if (statBoost.stat == Stat.Defense)
            return "방어력";
        else if (statBoost.stat == Stat.SpAttack)
            return "특수공격력";
        else if (statBoost.stat == Stat.SpDefense)
            return "특수방어력";
        else if (statBoost.stat == Stat.Speed)
            return "속도";
        else if (statBoost.stat == Stat.Accuracy)
            return "명중률";
        else if (statBoost.stat == Stat.Evasion)
            return "회피율";

        return null;
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            CalculateStats();
            return true;
        }

        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumOfMoves) return;

        Moves.Add(new Move(moveToLearn));
    }


    public bool HasMove(MoveBase moveToCheck)
    {
        return Moves.Count(m => m.Base == moveToCheck) > 0;
    }

    public Evolution CheckForEvolution()
    { 
        return Base.Evolutions.FirstOrDefault(e => e.RequiredLevel <= level);
    }

    public void Evolve(Evolution evolution) 
    {
        _base = evolution.EvolvesInto;
        CalculateStats();
    }


    public void Heal()
    {
        HP = MaxHp;
        OnHPChanged?.Invoke();
    }

    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }

    public int MaxHp { get; private set; }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
            critical = 2f;

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        DecreaseHP(damage);

        return damageDetails;
    }

    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public void SetStatus(ConditionID conditionId)
    {
        if (Status != null) return;

        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name}가 {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }
    public void SetVolatileStatus(ConditionID conditionId)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name}가 {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
                canPerformMove = false;
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
                canPerformMove = false;
        }

        return canPerformMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
}
