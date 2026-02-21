using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RuneInventoryManager : MonoBehaviour
{
    public int maxEquippedSlots = 8;

    [System.Serializable]
    public class RuneStack
    {
        public RuneItem rune;
        public int quantity;

        public RuneStack(RuneItem rune, int qty)
        {
            this.rune = rune;
            quantity = qty;
        }
    }

    [Header("UI")]
    public List<GameObject> equippedRuneSlots = new List<GameObject>();
    public Transform inventoryParent;
    public GameObject RuneUIPrefab;

    [Header("Data")]
    public List<RuneStack> inventoryRunes = new List<RuneStack>();
    public List<RuneItem> equippedRunes = new List<RuneItem>();

    public static RuneInventoryManager Instance;
    public RuneDatabase runeDatabase;

    public GameObject infoParent;

    public double RuneGemsBoost = 0;
    public double RuneMoneyBoost = 0;
    public double RuneLuckBoost = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize inventory and UI
        RefreshInventoryRunes();
        RefreshInventoryUI();
        RefreshEquippedUI();
    }

    /// <summary>
    /// Equip a rune from the inventory
    /// </summary>
    public void EquipRune(RuneItem rune)
    {
        if (equippedRunes.Count >= maxEquippedSlots)
        {
            Debug.Log("No available equipped slots!");
            return;
        }

        // Subtract from inventory
        RuneStack stack = inventoryRunes.Find(r => r.rune.runeId == rune.runeId);
        if (stack == null || stack.quantity <= 0)
        {
            Debug.Log("Rune not found in inventory!");
            return;
        }

        stack.quantity--;
        if (stack.quantity <= 0)
            inventoryRunes.Remove(stack);

        // Add to equipped list
        equippedRunes.Add(rune);

        /// Refresh UI
        RefreshInventoryUI();
        RefreshEquippedUI();

        Debug.Log("Equipped " + rune.runeName);
    }

    /// <summary>
    /// Unequip a rune at a specific slot
    /// </summary>
    public void UnequipRuneAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedRunes.Count)
            return;

        RuneItem rune = equippedRunes[slotIndex];
        equippedRunes.RemoveAt(slotIndex);

        // Add back to inventory
        RuneStack stack = inventoryRunes.Find(r => r.rune.runeId == rune.runeId);
        if (stack != null)
            stack.quantity++;
        else
            inventoryRunes.Add(new RuneStack(rune, 1));

        RefreshInventoryUI();
        RefreshEquippedUI();
        //RuneMustiplayer();
        Debug.Log("Unequipped " + rune.runeName);
    }

    /// <summary>
    /// Populate inventory list from existing UI buttons in inventoryParent
    /// </summary>
    public void RefreshInventoryRunes()
    {
        inventoryRunes.Clear();

        foreach (Transform child in inventoryParent)
        {
            RuneButton rb = child.GetComponent<RuneButton>();
            if (rb != null && rb.runeItem != null)
            {
                RuneStack stack = inventoryRunes.Find(r => r.rune.runeId == rb.runeItem.runeId);
                if (stack != null)
                    stack.quantity++;
                else
                    inventoryRunes.Add(new RuneStack(rb.runeItem, 1));
            }
        }

        Debug.Log("Inventory refreshed from UI. Total rune types: " + inventoryRunes.Count);
    }

    /// <summary>
    /// Refresh inventory UI to match inventoryRunes
    /// </summary>
    public void RefreshInventoryUI()
    {
        // Clear old UI
        foreach (Transform child in inventoryParent)
            Destroy(child.gameObject);

        foreach (RuneStack stack in inventoryRunes)
        {
            for (int i = 0; i < stack.quantity; i++)
            {
                GameObject newRuneUI = Instantiate(RuneUIPrefab, inventoryParent);
                newRuneUI.transform.localScale = Vector3.one;
                newRuneUI.transform.localPosition = Vector3.zero;

                RuneButton rb = newRuneUI.GetComponent<RuneButton>();
                rb.Setup(stack.rune, true); // true = inventory button
            }
        }
    }

    /// <summary>
    /// Refresh equipped UI to match equippedRunes
    /// </summary>
    public void RefreshEquippedUI()
    {
        for (int i = 0; i < equippedRuneSlots.Count; i++)
        {
            GameObject slot = equippedRuneSlots[i];

            // Clear old UI
            foreach (Transform child in slot.transform)
                Destroy(child.gameObject);

            if (i < equippedRunes.Count)
            {
                GameObject newRuneUI = Instantiate(RuneUIPrefab, slot.transform);
                newRuneUI.transform.localScale = Vector3.one;
                newRuneUI.transform.localPosition = Vector3.zero;

                RuneButton rb = newRuneUI.GetComponent<RuneButton>();
                rb.Setup(equippedRunes[i], false, i); // false = equipped button, slot index
                StatsUpdate(newRuneUI.GetComponent<RuneButton>().runeItem);
            }
        }
        RuneMustiplayer();
    }




    public void SaveRunesToData()
    {
        if (SaveDataController.currentData == null) return;

        // Clear old data
        SaveDataController.currentData.inventoryRunes.Clear();
        SaveDataController.currentData.equippedRuneIds.Clear();

        // Save inventory
        foreach (var stack in inventoryRunes)
        {
            SaveDataController.currentData.inventoryRunes.Add(new RuneData
            {
                runeId = stack.rune.runeId,
                quantity = stack.quantity
            });
        }

        // Save equipped runes
        foreach (var rune in equippedRunes)
        {
            SaveDataController.currentData.equippedRuneIds.Add(rune.runeId);
        }
    }

    public void LoadRunesFromData(List<RuneItem> allRuneDefinitions)
    {
        inventoryRunes.Clear();
        equippedRunes.Clear();

        var data = SaveDataController.currentData;
        if (data == null) return;

        // Load inventory runes
        foreach (var runeData in data.inventoryRunes)
        {
            RuneItem runeItem = allRuneDefinitions.Find(r => r.runeId == runeData.runeId);
            if (runeItem != null)
            {
                inventoryRunes.Add(new RuneStack(runeItem, runeData.quantity));
            }
        }

        // Load equipped runes
        foreach (var runeId in data.equippedRuneIds)
        {
            RuneItem runeItem = allRuneDefinitions.Find(r => r.runeId == runeId);
            if (runeItem != null)
            {
                equippedRunes.Add(runeItem);
            }
        }

        // Refresh UI
        RefreshInventoryUI();
        RefreshEquippedUI();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddRandomRuneToInventory();
            print("Added random rune to inventory for testing.");
        }
    }

    public void AddRandomRuneToInventory()
    {
        if (runeDatabase == null || runeDatabase.allRunes.Count == 0)
        {
            Debug.LogWarning("RuneDatabase is empty or not assigned!");
            return;
        }

        // Pick a random rune from the database
        int randomIndex = Random.Range(0, runeDatabase.allRunes.Count);
        RuneItem randomRune = runeDatabase.allRunes[randomIndex];

        // Add to inventory (stack if it exists)
        RuneStack stack = inventoryRunes.Find(r => r.rune.runeId == randomRune.runeId);
        if (stack != null)
        {
            stack.quantity++;
        }
        else
        {
            inventoryRunes.Add(new RuneStack(randomRune, 1));
        }

        // Refresh inventory UI
        RefreshInventoryUI();

        Debug.Log("Added random rune to inventory: " + randomRune.runeName);
    }

    public void StatsUpdate(RuneItem rune)
    {
        infoParent.transform.GetChild(0).transform.GetComponent<TMP_Text>().text = rune.runeName;
        infoParent.transform.GetChild(1).transform.GetComponent<TMP_Text>().text = rune.type.ToString();
        infoParent.transform.GetChild(2).transform.GetComponent<TMP_Text>().text = rune.rarity.ToString();
        infoParent.transform.GetChild(3).transform.GetComponent<TMP_Text>().text = rune.boostmultiplier.ToString();
    }

    public void RuneMustiplayer()
    {
        //anthony, the multiplayer is not going to 0, thats because equipped rune list is never null, so i puut a count to reset it 
        double moreGems = 0;
        double moreMoney = 0;
        double moreLuck = 0;
        //this  works its reseting when theres no runes pls dont reset it PLS DONT CHANGE AHHHHHHH 
        
        if(equippedRunes.Count == 0)
        {
            RuneGemsBoost = moreGems;
            RuneMoneyBoost = moreMoney;
            RuneLuckBoost = moreLuck;
            return;
        }
        foreach (var rune in equippedRunes)
        {
            // Example: Apply boost multiplier to player stats
            switch(rune.type)
            {
                case runeType.MoreGems:
                    moreGems += rune.boostmultiplier;
                    break;
                case runeType.MoreMoney:
                    moreMoney += rune.boostmultiplier;
                    break;
                case runeType.MoreLuck:
                    moreLuck += rune.boostmultiplier;
                    break;
                    // Add more cases as needed
            }
            // playerStats.ApplyBoost(boost);
            Debug.LogError(moreGems);
            Debug.LogError(moreMoney);
            Debug.LogError(moreLuck);

            RuneGemsBoost = moreGems;
            RuneMoneyBoost = moreMoney;
            RuneLuckBoost = moreLuck;
        }
    }

}