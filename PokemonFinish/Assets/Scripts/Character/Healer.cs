using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;

        yield return DialogManager.Instance.ShowDialogText("피곤해보이는구나! 여기서 쉬고가렴!",
            choices: new List<string>() { "네", "아니요" },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0) 
        {
            // Yes
            yield return Fader.i.FadeIn(0.5f);

            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(p => p.Heal());
            playerParty.PartyUpdated();

            yield return Fader.i.FadeOut(0.5f);

            yield return DialogManager.Instance.ShowDialogText($"모두 건강해졌네!! 몸 조심해!");
        }
        else if (selectedChoice == 1) 
        {
            // No
            yield return DialogManager.Instance.ShowDialogText($"그래! 마음이 바뀌면 다시 돌아오렴!");
        }

        
    }
}
