using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        if (Random.Range(1, 101) <= 10)
        {
            player.Character.Animator.IsMoving = false;
            GameController.Instance.StartBattle();
        }
    }
}
