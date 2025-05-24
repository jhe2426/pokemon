using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Pokemon _pokemon;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        levelText.text = "Lv" + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);

        statusColors = new Dictionary<ConditionID, Color>()
        {
            { ConditionID.psn, psnColor },
            { ConditionID.brn, brnColor },
            { ConditionID.slp, slpColor },
            { ConditionID.par, parColor },
            { ConditionID.frz, frzColor },
        };

        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;
    }

    void SetStatusText()
    {
        if (_pokemon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            string koreaStatus = GetKoreaStatus(_pokemon.Status.Id);
            statusText.text = koreaStatus;
            statusText.color = statusColors[_pokemon.Status.Id];
        }
    }

    private string GetKoreaStatus(ConditionID conditionId)
    {
        if (conditionId == ConditionID.psn)
            return "독";
        else if (conditionId == ConditionID.brn)
            return "화상";
        else if (conditionId == ConditionID.par)
            return "마비";
        else if (conditionId == ConditionID.frz)
            return "얼음";
        else if (conditionId == ConditionID.slp)
            return "잠";

        return "";
    }

    public IEnumerator UpdateHp()
    {
        if (_pokemon.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
            _pokemon.HpChanged = false;
        }
    }
}
