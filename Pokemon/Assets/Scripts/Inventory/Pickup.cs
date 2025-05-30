using System.Collections;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable
{
    [SerializeField] ItemBase item;

    public bool Used { get; set; } = false;

    public IEnumerator Interact(Transform initiator)
    {
        if (!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(item);

            Used = true;

            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;

            yield return DialogManager.Instance.ShowDialogText($"플레이어가 {item.Name}을(를) 발견했다!");
        }
    }
}
