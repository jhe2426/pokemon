using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum ShopState { Menu, Buying, Selling, Busy }

public class ShopController : MonoBehaviour
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] ShopUI shopUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;

    public event Action OnStart;
    public event Action OnFinish;

    ShopState state;

    Merchant Merchant;

    public static ShopController i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    Inventory inventory;
    private void Start()
    {
        inventory = Inventory.GetInventory();
    }

    public IEnumerator StartTrading(Merchant merchant)
    {
        this.Merchant = merchant;

        OnStart?.Invoke();
        yield return StartMenuState();
    }

    IEnumerator StartMenuState()
    {
        state = ShopState.Menu;

        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("������ ���͵帱���?",
               waitForInput: false,
               choices: new List<string>() { "���", "�ȴ�", "������" },
               onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            state = ShopState.Buying;
            walletUI.Show();
            shopUI.Show(Merchant.AvailableItems, (item) => StartCoroutine(BuyItem(item)),
                OnBackFromBuying);
        }
        else if (selectedChoice == 1)
        {
            // Sell
            state = ShopState.Selling;
            inventoryUI.gameObject.SetActive(true);
        }
        else if (selectedChoice == 2)
        {
            // Quit
            OnFinish?.Invoke();
            yield break;
        }
    }

    public void HandleUpdate()
    {
        if (state == ShopState.Selling)
        {
            inventoryUI.HandleUpdate(OnBackFromSelling, (selectedItem) => StartCoroutine(SellItem(selectedItem)));
        }
        else if (state == ShopState.Buying)
        {
            shopUI.HandleUpdate();
        }
    }

    void OnBackFromSelling()
    {
        inventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMenuState());
    }

    IEnumerator SellItem(ItemBase item)
    {
        state = ShopState.Busy;

        if (!item.IsSellable)
        {
            yield return DialogManager.Instance.ShowDialogText("���� �����ϴ�!");
            state = ShopState.Selling;
            yield break;
        }

        walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price / 2);
        int countToSell = 1;

        int itemCount = inventory.GetItemCount(item);
        if (itemCount > 1)
        {
            yield return DialogManager.Instance.ShowDialogText("� �Ȱǰ���?",
                waitForInput: false, autoClose: false);

            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice,
                (selectedCount) => countToSell = selectedCount);

            DialogManager.Instance.CloseDialog();
        }

        sellingPrice = sellingPrice * countToSell;

        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText($"{sellingPrice}�� �Դϴ�! �Ľðھ��?",
               waitForInput: false,
               choices: new List<string>() { "��", "�ƴϿ�" },
               onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            // Yes
            inventory.RemoveItem(item, countToSell);
            Wallet.i.AddMoney(sellingPrice);
            // TODO: Add selling price to player's wallet
            yield return DialogManager.Instance.ShowDialogText($"{item.Name}�� �Ȱ�{sellingPrice}���� �޾Ҵ�!");
        }

        walletUI.Close();

        state = ShopState.Selling;
    }

    IEnumerator BuyItem(ItemBase item)
    {
        state = ShopState.Busy;

        yield return DialogManager.Instance.ShowDialogText("� ��ǰǰ���?",
            waitForInput: false, autoClose: false);

        int countToBuy = 1;
        yield return countSelectorUI.ShowSelector(100, item.Price,
            (selectedCount) => countToBuy = selectedCount);

        DialogManager.Instance.CloseDialog();

        float totalPrice = item.Price * countToBuy;

        if (Wallet.i.HasMoney(totalPrice))
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"�� {totalPrice}�� �Դϴ�! �����ϰڽ��ϱ�?",
                   waitForInput: false,
                   choices: new List<string>() { "��", "�ƴϿ�" },
                   onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

            if (selectedChoice == 0)
            {
                inventory.AddItem(item, countToBuy);
                Wallet.i.TakeMoney(totalPrice);
                yield return DialogManager.Instance.ShowDialogText("�̿����ּż� �����մϴ�!");
            }
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText("���� ������� �ʴ�..");
        }

        state = ShopState.Buying;
    }

    void OnBackFromBuying()
    {
        shopUI.Close();
        walletUI.Close();
        StartCoroutine(StartMenuState());
    }
}