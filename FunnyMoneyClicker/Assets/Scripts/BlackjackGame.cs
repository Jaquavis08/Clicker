using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple Blackjack game mode:
/// - Call StartRound(bet) to begin (bet is deducted immediately).
/// - Call Hit() or Stand() while round is active.
/// - On win the player receives double the bet (bet*2 added), on push the bet is refunded.
/// - Integrate UI by subscribing to the public actions.
/// </summary>
public class BlackjackGame : MonoBehaviour
{
    public int minBet = 1;

    // Events for UI integration
    public Action<string> OnStatusUpdated;           // e.g. "You Win", "You Lose", "Push"
    public Action<List<int>, List<int>, bool> OnHandsUpdated; // playerHand, dealerHand, isDealerHoleHidden

    private List<int> deck;
    private List<int> playerHand;
    private List<int> dealerHand;

    private int currentBet;
    private bool roundActive;

    private void Awake()
    {
        playerHand = new List<int>();
        dealerHand = new List<int>();
        InitDeck();
    }

    private void InitDeck()
    {
        deck = new List<int>();
        // Use one deck (1..13 values) four suits -> 4 * 13 cards
        for (int s = 0; s < 4; s++)
        {
            for (int v = 1; v <= 13; v++)
                deck.Add(v);
        }
        ShuffleDeck();
    }

    private void ShuffleDeck()
    {
        var rng = new System.Random();
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int tmp = deck[k];
            deck[k] = deck[n];
            deck[n] = tmp;
        }
    }

    private int DrawCard()
    {
        if (deck.Count == 0)
        {
            InitDeck();
        }
        int card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    private int CardValue(int card)
    {
        if (card >= 10) return 10; // 10, J, Q, K
        return card; // Ace is 1 here; treated later as 1 or 11
    }

    private int BestHandValue(List<int> hand)
    {
        int sum = 0;
        int aceCount = 0;
        foreach (var c in hand)
        {
            if (c == 1) { aceCount++; sum += 1; }
            else sum += CardValue(c);
        }
        // Try to promote some aces from 1 to 11 while <=21
        for (int i = 0; i < aceCount; i++)
        {
            if (sum + 10 <= 21) sum += 10;
        }
        return sum;
    }

    private bool IsBlackjack(List<int> hand)
    {
        return hand.Count == 2 && BestHandValue(hand) == 21;
    }

    // Public API
    public bool StartRound(int bet)
    {
        if (roundActive)
        {
            OnStatusUpdated?.Invoke("Round already active.");
            return false;
        }

        if (bet < minBet)
        {
            OnStatusUpdated?.Invoke($"Minimum bet is {minBet}.");
            return false;
        }

        int playerMoney = (int)SaveDataController.currentData.moneyCount;
        if (bet > playerMoney)
        {
            OnStatusUpdated?.Invoke("Not enough money to bet.");
            return false;
        }

        currentBet = bet;
        // Deduct bet immediately
        SaveDataController.currentData.moneyCount -= currentBet;
        if (SaveDataController.currentData.moneyCount < 0) SaveDataController.currentData.moneyCount = 0;

        playerHand.Clear();
        dealerHand.Clear();

        // Deal initial cards
        playerHand.Add(DrawCard());
        dealerHand.Add(DrawCard());
        playerHand.Add(DrawCard());
        dealerHand.Add(DrawCard());

        roundActive = true;

        // Notify UI (dealer hole hidden)
        OnHandsUpdated?.Invoke(new List<int>(playerHand), new List<int>(dealerHand), true);

        // Immediate blackjack checks
        bool playerBJ = IsBlackjack(playerHand);
        bool dealerBJ = IsBlackjack(dealerHand);
        if (playerBJ || dealerBJ)
        {
            // Reveal dealer
            OnHandsUpdated?.Invoke(new List<int>(playerHand), new List<int>(dealerHand), false);

            if (playerBJ && !dealerBJ)
                EndRound(true, "Blackjack! You win.");
            else if (!playerBJ && dealerBJ)
                EndRound(false, "Dealer has Blackjack. You lose.");
            else
                EndRound(null, "Push (both Blackjack).");
        }
        else
        {
            OnStatusUpdated?.Invoke("Round started. Choose Hit or Stand.");
        }

        return true;
    }

    public void Hit()
    {
        if (!roundActive)
        {
            OnStatusUpdated?.Invoke("No active round.");
            return;
        }

        playerHand.Add(DrawCard());
        OnHandsUpdated?.Invoke(new List<int>(playerHand), new List<int>(dealerHand), true);

        int playerVal = BestHandValue(playerHand);
        if (playerVal > 21)
        {
            // Bust
            OnHandsUpdated?.Invoke(new List<int>(playerHand), new List<int>(dealerHand), false);
            EndRound(false, $"Bust ({playerVal}). You lose.");
        }
    }

    public void Stand()
    {
        if (!roundActive)
        {
            OnStatusUpdated?.Invoke("No active round.");
            return;
        }

        // Reveal dealer and play dealer rules: hit until 17 or higher
        OnHandsUpdated?.Invoke(new List<int>(playerHand), new List<int>(dealerHand), false);

        while (BestHandValue(dealerHand) < 17)
        {
            dealerHand.Add(DrawCard());
            OnHandsUpdated?.Invoke(new List<int>(playerHand), new List<int>(dealerHand), false);
        }

        int playerVal = BestHandValue(playerHand);
        int dealerVal = BestHandValue(dealerHand);

        if (dealerVal > 21)
        {
            EndRound(true, $"Dealer busts ({dealerVal}). You win.");
            return;
        }

        if (playerVal > dealerVal)
            EndRound(true, $"You win ({playerVal} vs {dealerVal}).");
        else if (playerVal < dealerVal)
            EndRound(false, $"You lose ({playerVal} vs {dealerVal}).");
        else
            EndRound(null, $"Push ({playerVal} vs {dealerVal}).");
    }

    private void EndRound(bool? playerWon, string message)
    {
        // playerWon: true = player win, false = player lose, null = push
        if (playerWon == true)
        {
            // Win: player "wins double" -> add bet * 2 (so net gain = bet)
            SaveDataController.currentData.moneyCount += currentBet * 2;
            OnStatusUpdated?.Invoke(message + $" +{currentBet * 2} coins awarded.");
        }
        else if (playerWon == false)
        {
            // Lose: bet already deducted
            OnStatusUpdated?.Invoke(message + " Bet lost.");
        }
        else
        {
            // Push: refund bet
            SaveDataController.currentData.moneyCount += currentBet;
            OnStatusUpdated?.Invoke(message + " Bet refunded.");
        }

        // clamp
        if (SaveDataController.currentData.moneyCount < 0) SaveDataController.currentData.moneyCount = 0;

        roundActive = false;
        currentBet = 0;
        // Final hands update (revealed)
        OnHandsUpdated?.Invoke(new List<int>(playerHand), new List<int>(dealerHand), false);
    }

    // Optional helper to expose player's money
    public int GetPlayerMoney() => (int)SaveDataController.currentData.moneyCount;
}