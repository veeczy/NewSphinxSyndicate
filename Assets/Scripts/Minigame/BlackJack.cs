using JetBrains.Annotations;
using NUnit.Framework;
using TMPro;
using Unity.Burst.Intrinsics;
//using UnityEditor.Experimental.GraphView;
//using UnityEditor.Rendering;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.U2D;

public class BlackJack : MonoBehaviour
{
    [Header("Game Settings")]
    public bool isTalking = false;
    public bool canMove = true;
    public bool playAgain = true;
    public bool gameActive = false;
    public bool playerNear = false;
    public bool resetCards = false;

    [Header("Black Jack Cards")]
    public Sprite[] cardSprites; //array of card sprites
    public int[] Cards = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 10 , 10,  2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 10, 10,  2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 10, 10, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 10, 10, 10 }; //cards in order
    //11 = Ace, after is J, K, Q (not value in game, just noting the value in their order in the sprites index)
    public int[] remainingCards; //duplicate array that will have cards removed as they are used

    [Header("Black Jack UI - Base")]
    public GameObject blackJackScreen;
    //dialogue
    public GameObject dialogueUI;
    public TMP_Text dialogueText;

    //spaces for player's cards
    [Header("Black Jack UI - Player's Cards")]
    public GameObject playerCardSpace1; 
    public GameObject playerCardSpace2;
    public GameObject playerCardSpace3;
    public GameObject playerCardSpace4;
    public GameObject playerCardSpace5;
    public GameObject playerCardSpace6;
    public GameObject playerCardSpace7;
    public GameObject playerCardSpace8;

    public GameObject playerCardSumPanel;
    public TMP_Text playerCardSum;

    [Header("Black Jack UI - Dealer's Cards")]
    public GameObject dealerHideCard;
    public GameObject dealerCardSpace1;
    public GameObject dealerCardSpace2;
    public GameObject dealerCardSpace3;
    public GameObject dealerCardSpace4;
    public GameObject dealerCardSpace5;
    public GameObject dealerCardSpace6;
    public GameObject dealerCardSpace7;
    public GameObject dealerCardSpace8;

    public GameObject dealerCardSumPanel;
    public TMP_Text dealerCardSum;

    [Header("Black Jack UI - Buttons")]
    public GameObject button1; //button that says yes 
    public GameObject button2; //button that says no
    public GameObject button3; //button for Hit
    public GameObject button4; //button for stand

    public GameObject betGroup;
    public GameObject button5; //button for betx1
    public GameObject button6; //button for betx10
    public GameObject button7; //button for betx100
    public GameObject button8; //button for betxMAX

    public GameObject buttonDeal; //button for dealing cards after betting

    [Header("Black Jack UI - Credits")]
    public GameObject creditsPanel;
    public GameObject creditsNumber1;
    public GameObject creditsNumber2;
    public GameObject creditsNumber3;
    public GameObject creditsNumber4;

    public GameObject betAmountPanel;
    public TMP_Text betAmountText;
    public GameObject BetChips;

    public GameObject buttonSumReveal;

    [Header("Black Jack Data")]
    public string[] dialogueLines = new string[] {"", "Do you want to play?", "You Win!", "You Lose!", "Do you want to play again?", "Tie!", "How much are you willing to bet?", "You don't have enough for that."}; //dialogue the dealer can say
    public int dialogueIndex = 0; //number to call what dialogue is said
    public int cardsDealt = 0; //numvber to track how many cards have been dealt
    private int cardsCounted = 0;

    public int randomCard; //number randomly generated to pick index
    public int randomCardValue; // randomly generated card's value
    public List<int> dealerHand; //array to track what cards are in the dealer's hand
    public int dealerHandValue; //sum of dealer hand
    public List<int> playerHand; //array to track what cards are in the player's hand
    public int playerHandValue; //sum of player hand

    public bool canHit = true; //as long as the player hasn't bust they can hit and get another card
    public bool bust = false; //met if player hand has sum that is over 21
    public bool onWinLose = false;
    public bool showSum = false;
    public bool betScreen = false;

    [Header("Black Jack Data - Credits")]
    public int credits;
    public int multiplier = 1;
    public bool takeMoney = false;
    public int perfectScore = 0;

    public Sprite[] creditSprites; //sprites for credits
    public string creditsString;

    public int ones;
    public int tens;
    public int hundreds;
    public int thousands;

    [Header("Black Jack Data - Betting")]
    public int betAmount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize
        InitializeGameObjects(); //collect all things to inspector if they aren't there yet
        ResetCardDeck(); //reset data for card deck
        credits = PlayerPrefs.GetInt("credits"); //grab credits from player prefs
    }

    // Update is called once per frame
    void Update()
    {
        PlayerPrefs.SetInt("credits", credits); //update credits when needed
        LoadCreditsUI();

        if (playerNear && Input.GetButtonDown("Interact"))
        {
            isTalking = true;
        }

        if (Input.GetButtonDown("Cancel"))
        {
            EndGame();
        }

        resetCards = CountCardsRemaining();
        if (resetCards) { ResetCardDeck(); }

        playerCardSum.text = playerHandValue.ToString();
        betAmountText.text = betAmount.ToString();

        //if(!showSum) { HideUI(playerCardSumPanel); HideUI(dealerCardSumPanel); } //if player decides at anytime to hide the sum

        
        if (isTalking && !gameActive) //if talking to the Black Jack NPC
        {
            playAgain = true;
            canMove = false; // you don't want player to be able to move around while Gambling so need to freeze movement
            if(betScreen)
            {
                if (betAmount <= credits) { dialogueIndex = 6; StartDialogue(); } //Update Dialogue "How much do you want to bet?"
                if (betAmount > credits) { dialogueIndex = 7; StartDialogue(); } //Update Dialogue "You don't have enough for that."
            }
            if (!betScreen) { PlayAgain(1); } // Ask if you want to Play
            
            //if(!playAgain) { HideUI(blackJackScreen); } //hides all ui related to blackjack
        }
        if (!isTalking) { canMove = true; } // return movement if not talking to minigame npc

        if(gameActive) //space for active gameplay and things during it that need to be frequently checked on update -- past start
        {
            //buttons for hit me/ stay
            if(!bust)
            {
                //each turn check status
                playerHandValue = playerHand.Sum(); //calculate sum of player hand
                dealerHandValue = dealerHand.Sum(); //calculate sum of dealer hand
                if (playerHandValue > 21) { bust = true; } //if above 21 then bust
                if(playerHandValue == 21 && cardsDealt == 2) //if win on dealt
                {
                    WinGame();
                }
                if(playerHandValue == 21 && cardsDealt > 2) //if dealt to 21
                {
                    Stand();
                }
                if(playerHandValue < 21)
                {
                    if (!onWinLose) { GameScreen(); }
                    bust = false;
                }
            }

            if(bust) //lose the game
            {   
                HideUI(dealerHideCard);
                if (!onWinLose) { LoseGame(); }
            }

            if(onWinLose) //if on win lose screen, you need input to move forward
            {
                if (Input.GetButton("Shoot") || Input.GetButton("Interact"))
                {
                    //move forward
                    PlayAgain(4);
                }
            }
        }
    }

    //**SYSTEM FUNCTIONS**//
    public void ShowUI(GameObject UI)
    {
        UI.SetActive(true);
    }

    public void HideUI(GameObject UI)
    {
        UI.SetActive(false);
    }

    private GameObject FindInactiveObjectByName(string name)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in objects)
        {
            if (obj.name == name && obj.scene.isLoaded)
                return obj;
        }

        return null;
    }

    public void InitializeGameObjects() //if anything is not linked in inspector, it will now be found
    {
        //BlackJack UI - BASE
        if (blackJackScreen == null) { blackJackScreen = FindInactiveObjectByName("BlackJackScreen"); }
        if (dialogueUI == null) { dialogueUI = FindInactiveObjectByName("DialogueBoxPanel-BJ"); }
        if (dialogueText == null)
        {
            GameObject placeholder = FindInactiveObjectByName("Dialogue-BlackJack");
            dialogueText = placeholder.GetComponent<TextMeshProUGUI>();
        }

        //BlackJack UI - CREDITS
        if (creditsPanel == null) { creditsPanel = FindInactiveObjectByName("CreditsBG"); }
        if (creditsNumber1 == null) { creditsNumber1 = FindInactiveObjectByName("Credits-Number1"); }
        if (creditsNumber2 == null) { creditsNumber2 = FindInactiveObjectByName("Credits-Number2"); }
        if (creditsNumber3 == null) { creditsNumber3 = FindInactiveObjectByName("Credits-Number3"); }
        if (creditsNumber4 == null) { creditsNumber4 = FindInactiveObjectByName("Credits-Number4"); }

        if (betAmountPanel == null) { betAmountPanel = FindInactiveObjectByName("betAmountPanel"); }
        if (betAmountText == null)
        {
            GameObject placeholder = FindInactiveObjectByName("betAmountText");
            betAmountText = placeholder.GetComponent<TextMeshProUGUI>();
        }

        //BlackJack UI - Dealer's Cards
        if (dealerCardSpace1 == null) { dealerCardSpace1 = FindInactiveObjectByName("dc-1"); }
        if (dealerCardSpace2 == null) { dealerCardSpace2 = FindInactiveObjectByName("dc-2"); }
        if (dealerCardSpace3 == null) { dealerCardSpace3 = FindInactiveObjectByName("dc-3"); }
        if (dealerCardSpace4 == null) { dealerCardSpace4 = FindInactiveObjectByName("dc-4"); }
        if (dealerCardSpace5 == null) { dealerCardSpace5 = FindInactiveObjectByName("dc-5"); }
        if (dealerCardSpace6 == null) { dealerCardSpace6 = FindInactiveObjectByName("dc-6"); }
        if (dealerCardSpace7 == null) { dealerCardSpace7 = FindInactiveObjectByName("dc-7"); }
        if (dealerCardSpace8 == null) { dealerCardSpace8 = FindInactiveObjectByName("dc-8"); }

        if (dealerHideCard == null) { dealerHideCard = FindInactiveObjectByName("Dealer Hide Card"); }
        if (dealerCardSumPanel == null) { dealerCardSumPanel = FindInactiveObjectByName("dc-SumPanel"); }
        if (dealerCardSum == null)
        {
            GameObject placeholder = FindInactiveObjectByName("dc-Sum");
            dealerCardSum = placeholder.GetComponent<TextMeshProUGUI>();
        }

        //BlackJack UI - Player's Cards
        if (playerCardSpace1 == null) { playerCardSpace1 = FindInactiveObjectByName("pc-1"); }
        if (playerCardSpace2 == null) { playerCardSpace2 = FindInactiveObjectByName("pc-2"); }
        if (playerCardSpace3 == null) { playerCardSpace3 = FindInactiveObjectByName("pc-3"); }
        if (playerCardSpace4 == null) { playerCardSpace4 = FindInactiveObjectByName("pc-4"); }
        if (playerCardSpace5 == null) { playerCardSpace5 = FindInactiveObjectByName("pc-5"); }
        if (playerCardSpace6 == null) { playerCardSpace6 = FindInactiveObjectByName("pc-6"); }
        if (playerCardSpace7 == null) { playerCardSpace7 = FindInactiveObjectByName("pc-7"); }
        if (playerCardSpace8 == null) { playerCardSpace8 = FindInactiveObjectByName("pc-8"); }

        if (playerCardSumPanel == null) { playerCardSumPanel = FindInactiveObjectByName("pc-SumPanel"); }
        if (playerCardSum == null)
        {
            GameObject placeholder = FindInactiveObjectByName("pc-Sum");
            playerCardSum = placeholder.GetComponent<TextMeshProUGUI>();
        }

        //BlackJack UI - Buttons
        if (button1 == null) { button1 = FindInactiveObjectByName("Button1"); }
        if (button2 == null) { button2 = FindInactiveObjectByName("Button2"); }
        if (button3 == null) { button3 = FindInactiveObjectByName("Button3"); }
        if (button4 == null) { button4 = FindInactiveObjectByName("Button4"); }

        //BlackJack UI - Betting Buttons
        if (betGroup == null) { betGroup = FindInactiveObjectByName("BET"); }
        if (button5 == null) { button5 = FindInactiveObjectByName("SlotsBG-Bet1"); }
        if (button6 == null) { button6 = FindInactiveObjectByName("SlotsBG-Bet10"); }
        if (button7 == null) { button7 = FindInactiveObjectByName("SlotsBG-Bet100"); }
        if (button8 == null) { button8 = FindInactiveObjectByName("SlotsBG-BetAll"); }

        if (BetChips == null ) { BetChips = FindInactiveObjectByName("BetChips"); }

        if (buttonDeal == null ) { buttonDeal = FindInactiveObjectByName("ButtonDeal"); }
        if (buttonSumReveal == null) { buttonSumReveal = FindInactiveObjectByName("ButtonSumReveal"); }
    }

    public bool ReturnMovement()
    {
        return canMove;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
            playerNear = true;
        Debug.Log("OnCollisionEnter2D");
        Debug.Log(other.gameObject.name + " : " + gameObject.name + " : " + Time.time);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNear = false;
        Debug.Log("OnCollisionExit2D");
        Debug.Log(other.gameObject.name + " : " + gameObject.name + " : " + Time.time);
    }

    //**CREDITS FUNCTIONS**
    public void LoadCreditsUI()
    {
        UpdateCreditsSprites(); //update the sprites for UI before showing and revealing them

        if (credits <= -100) { ShowUI(creditsNumber4); ShowUI(creditsNumber3); ShowUI(creditsNumber2); ShowUI(creditsNumber1); } //if credits in hundred or over but negative

        if (credits <= -10 && credits > -100) { ShowUI(creditsNumber1); ShowUI(creditsNumber2); ShowUI(creditsNumber3); HideUI(creditsNumber4); } //if credits in tens space but negative

        if (credits < 0 && credits > -10) { ShowUI(creditsNumber1); ShowUI(creditsNumber2); HideUI(creditsNumber3); HideUI(creditsNumber4); } //if credits is in ones space but negative

        if (credits >= 0 && credits < 10) { ShowUI(creditsNumber1); HideUI(creditsNumber2); HideUI(creditsNumber3); HideUI(creditsNumber4); }//if credits is in ones space

        if (credits >= 10 && credits < 100) { ShowUI(creditsNumber1); ShowUI(creditsNumber2); HideUI(creditsNumber3); HideUI(creditsNumber4); } //if credits is in tens space show second digit

        if (credits >= 100) { ShowUI(creditsNumber1); ShowUI(creditsNumber2); ShowUI(creditsNumber3); HideUI(creditsNumber4); }  //if credits is in hundreds space show third digit

        if (credits >= 1000) { ShowUI(creditsNumber1); ShowUI(creditsNumber2); ShowUI(creditsNumber3); ShowUI(creditsNumber4); } //if credits is in thousands space
    }

    public void UpdateCreditsSprites()
    {
        credits = PlayerPrefs.GetInt("credits");
        UpdateCreditsDigits();

        if (credits <= -100) //if credits in hundred or over but negative
        {
            creditsNumber4.GetComponent<Image>().sprite = creditSprites[10];
            creditsNumber3.GetComponent<Image>().sprite = creditSprites[tens];
            creditsNumber2.GetComponent<Image>().sprite = creditSprites[hundreds];
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[thousands];
        }

        if (credits <= -10 && credits > -100) //if credits in tens space but negative
        {
            creditsNumber3.GetComponent<Image>().sprite = creditSprites[10];
            creditsNumber2.GetComponent<Image>().sprite = creditSprites[tens];
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[hundreds];
        }

        if (credits < 0 && credits >= -9) //if credits is in ones space but negative
        {
            creditsNumber2.GetComponent<Image>().sprite = creditSprites[10];
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[tens];
        }

        if (credits < 10 && credits >= 0)
        {
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[ones]; //update credit number sprite
        }

        if (credits > 9 && credits < 100)
        {
            creditsNumber2.GetComponent<Image>().sprite = creditSprites[ones]; //update credit number sprite
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[tens]; //update credit number sprite
        }

        if (credits > 99 && credits < 1000)
        {
            creditsNumber3.GetComponent<Image>().sprite = creditSprites[ones]; //update credit number sprite
            creditsNumber2.GetComponent<Image>().sprite = creditSprites[tens]; //update credit number sprite
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[hundreds]; //update credit number sprite
        }

        if (credits > 999)
        {
            creditsNumber4.GetComponent<Image>().sprite = creditSprites[ones]; //update credit number sprite
            creditsNumber3.GetComponent<Image>().sprite = creditSprites[tens]; //update credit number sprite
            creditsNumber2.GetComponent<Image>().sprite = creditSprites[hundreds]; //update credit number sprite
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[thousands]; //update credit number sprite
        }

    }

    public void UpdateCreditsDigits()
    {
        creditsString = credits.ToString(); //convert to string

        //**IF POSITIVE**//
        if (creditsString.Length == 4) //thousands
        {
            ones = creditsString[0];
            ones = CharToInt(ones);

            tens = creditsString[1];
            tens = CharToInt(tens);

            hundreds = creditsString[2];
            hundreds = CharToInt(hundreds);

            thousands = creditsString[3];
            thousands = CharToInt(thousands);
        }

        if (creditsString.Length == 3) //hundreds
        {
            ones = creditsString[0];
            ones = CharToInt(ones);

            tens = creditsString[1];
            tens = CharToInt(tens);

            hundreds = creditsString[2];
            hundreds = CharToInt(hundreds);

            //thousands = 0;
        }

        if (creditsString.Length == 2) //tens
        {
            ones = creditsString[0];
            ones = CharToInt(ones);

            tens = creditsString[1];
            tens = CharToInt(tens);

            //hundreds = 0;
            thousands = 0;
        }

        if (creditsString.Length == 1) //ones
        {
            ones = creditsString[0];
            ones = CharToInt(ones);

            //tens = 0;
            hundreds = 0;
            thousands = 0;
        }
    }

    public int CharToInt(int character)
    {
        if (character == 48) { return 0; }
        if (character == 49) { return 1; }
        if (character == 50) { return 2; }
        if (character == 51) { return 3; }
        if (character == 52) { return 4; }
        if (character == 53) { return 5; }
        if (character == 54) { return 6; }
        if (character == 55) { return 7; }
        if (character == 56) { return 8; }
        if (character == 57) { return 9; }
        if (character == 2212) { return 10; }
        else { return 0; }
    }
    //**END CREDITS FUNCTIONS**

    public void BetX1()
    {
        betAmount--;
        if (betAmount < credits) { dialogueIndex = 7; StartDialogue(); }
    }

    public void RemoveBetx1()
    {
        if (credits - betAmount >= 0) { betAmount--; }
        if(betAmount < 0 || credits < 0) { betAmount = 0; }
        else { betAmount = 0; }
    }

    public void BetX10()
    {
        betAmount = betAmount + 10;
        if (betAmount > credits) { dialogueIndex = 7; StartDialogue(); }
    }

    public void RemoveBetx10()
    {
        if (credits - betAmount >= 0) { betAmount = betAmount - 10; }
        if(credits < 0 || betAmount < 0) { betAmount = 0; }
    }

    public void BetX100()
    {
        betAmount = betAmount + 100;
        if (betAmount > credits) { dialogueIndex = 7; StartDialogue(); }
    }

    public void RemoveBetx100()
    {
        if (credits - betAmount >= 0) { betAmount = betAmount - 100; }
        if (credits < 0 || betAmount < 0) { betAmount = 0; }
    }

    public void BetMax()
    {
        if (credits > 0) { betAmount = credits; }
        else { dialogueIndex = 7; StartDialogue(); }
    }

    public void RemoveBetMax()
    {
        betAmount = 0;
    }

    public void ShowSum()
    {
        showSum = !showSum;
        if(showSum) 
        {
            ShowUI(dealerCardSumPanel);
            ShowUI(playerCardSumPanel);
        }
        if(!showSum)
        {
            HideUI(dealerCardSumPanel);
            HideUI(playerCardSumPanel);
        }
    }

    public void ResetCardDeck()
    {
        for (int i = 0; i < Cards.Length; i++)
        {
            remainingCards[i] = Cards[i];
        }
        resetCards = false;
    }

    public bool CountCardsRemaining()
    {
        for (int i = 0; i < Cards.Length; i++)
        {
            if (Cards[i] != 0) { cardsCounted++; }
        }

        if(cardsCounted < 16)
        {
            cardsCounted = 0;
            return true; //if there is less than 16 cards in deck, reset deck
        }
        else { cardsCounted = 0; return false; }
    }

    //**END SYSTEM FUNCTIONS**//


    //**GAME MECHANICS**

    public void PlayAgain(int speech) //this is for when it asks if you want to play
    {
        
        ShowUI(blackJackScreen); //shows all ui related to blackjack
        ShowUI(button1); //takes you to Betting() where it lets you bet before playing
        ShowUI(button2); //exits game

        HideUI(dealerCardSumPanel);
        HideUI(playerCardSumPanel);
        HideUI(betAmountPanel);
        HideUI(buttonSumReveal);
        HideUI(creditsPanel);

        HideUI(button3);
        HideUI(button4);
        HideAllCards();

        gameActive = false;

        dialogueIndex = speech;
        StartDialogue();
        ShowUI(dialogueUI);
    }

    public void Betting() //this is for when betting, before game is dealt
    {
        betScreen = true;
        if(betAmount <= credits) { dialogueIndex = 6; } //Update Dialogue "How much do you want to bet?"
        if(betAmount > credits) { dialogueIndex = 7; } //Update Dialogue "You don't have enough for that." 
        StartDialogue(); //Show New Dialogue

        HideUI(button1); //no yes button
        HideUI(button2); //no no button
        ShowUI(buttonDeal); //show Deal! button
        ShowUI(betAmountPanel);
        ShowUI(BetChips);

        ShowUI(betGroup); //Show Gambling Chips
        ShowUI(creditsPanel); //show total credits so you know how much you can gamble

        HideUI(dealerCardSumPanel);
        HideUI(playerCardSumPanel);

        //if they press buttons, betAmount changes; then when they are done they press a button "Deal" that changes scene to Gameplay
    }

    public void DealButton()
    {
        if(betAmount < credits) { StartGame(); }
    }

    public void GameScreen() //this is where game screen ui is set up
    {
        //Set up GameScreen
        HideUI(button1); //hide button that sais yes
        HideUI(button2); //hide button that says no
        ShowUI(button3);
        ShowUI(button4);
        
        HideUI(dialogueUI); //hide dialogue box
        HideUI(buttonDeal); //after dealing cards, you hide button that says do you want to

        ShowUI(buttonSumReveal); //when deal is gone, sum reveal button is shown
        HideUI(betGroup); //show gambling chips still
        ShowUI(creditsPanel);
        HideUI(betAmountPanel);
        HideUI(BetChips);

        if(showSum) { ShowUI(dealerCardSumPanel); ShowUI(playerCardSumPanel); }
    }
    public void StartGame() //Gameplay
    {
        //this is where gameplay data is set up
        betScreen = false;
        gameActive = true;
        onWinLose = false;
        takeMoney = false;
        perfectScore = 0;
        dealerCardSum.text = "?"; //reset hiding dealer sum
        ResetGame(); //reset all game data to ensure clean start
        GameScreen();

        Deal(0); //initial deal player card 1
        playerCardSpace1.GetComponent<Image>().sprite = cardSprites[randomCard]; //update card sprite
        Deal(0); //initial deal card 2
        playerCardSpace2.GetComponent<Image>().sprite = cardSprites[randomCard]; //update card sprite
        //initial deal show player cards
        ShowUI(playerCardSpace1);
        ShowUI(playerCardSpace2);

        Deal(1); //initial deal dealer card 1
        dealerCardSpace1.GetComponent<Image>().sprite = cardSprites[randomCard]; //update card sprite
        Deal(1); //initial deal dealer card 2
        dealerCardSpace2.GetComponent<Image>().sprite = cardSprites[randomCard]; //update card sprite
        //initial deal show dealer cards
        ShowUI(dealerHideCard);
        ShowUI(dealerCardSpace1);
        ShowUI(dealerCardSpace2);

        cardsDealt = 2;
        if (cardsDealt  == 2) { credits = credits - betAmount; } //take away your bet if the game is dealt
    }

    public void Deal(int i)
    {
        RandomDraw();
        if (remainingCards[randomCard] != 0)
        {
            randomCardValue = remainingCards[randomCard]; //save value of card
            remainingCards[randomCard] = 0; //null that card space so it can't be drawn again
            if(i == 0) { playerHand.Add(randomCardValue); } //if player turn
            if (i == 1) { dealerHand.Add(randomCardValue); } //if dealer turn
        }
        else Deal(i);
    }

    public void RandomDraw()
    {
        randomCard = Random.Range(0, 51); //index has to be less than 52 because array is 52 so array[52] = error
    }

    public void Hit()
    {
        if(cardsDealt == 2)
        {
            Deal(0);
            playerCardSpace3.GetComponent<Image>().sprite = cardSprites[randomCard];
            ShowUI(playerCardSpace3);
            cardsDealt++;
            takeMoney = false;
            return;
        }
        if(cardsDealt == 3)
        {
            Deal(0);
            playerCardSpace4.GetComponent<Image>().sprite = cardSprites[randomCard];
            ShowUI(playerCardSpace4);
            cardsDealt++;
            takeMoney = false;
            return;
        }
        if (cardsDealt == 4)
        {
            Deal(0);
            playerCardSpace5.GetComponent<Image>().sprite = cardSprites[randomCard];
            ShowUI(playerCardSpace5);
            cardsDealt++;
            takeMoney = false;
            return;
        }
        if (cardsDealt == 5)
        {
            Deal(0);
            playerCardSpace6.GetComponent<Image>().sprite = cardSprites[randomCard];
            ShowUI(playerCardSpace6);
            cardsDealt++;
            takeMoney = false;
            return;
        }
        if (cardsDealt == 6)
        {
            Deal(0);
            playerCardSpace7.GetComponent<Image>().sprite = cardSprites[randomCard];
            ShowUI(playerCardSpace7);
            cardsDealt++;
            takeMoney = false;
            return;
        }
        if (cardsDealt == 7)
        {
            Deal(0);
            playerCardSpace8.GetComponent<Image>().sprite = cardSprites[randomCard];
            ShowUI(playerCardSpace8);
            cardsDealt++;
            takeMoney = false;
            return;
        }
    }

    public void ShowDealerCards()
    {
        if(dealerHand.Count == 2)
        {
            Deal(1);
            dealerCardSpace3.GetComponent<Image>().sprite = cardSprites[randomCard];
            ShowUI(dealerCardSpace3);
            return;
        }
        if (dealerHand.Count == 3)
        {
            Deal(1);
            dealerCardSpace4.GetComponent<Image>().sprite = cardSprites[randomCard];
            ShowUI(dealerCardSpace4);
            return;
        }
        if (dealerHand.Count == 4)
        {
            Deal(1);
            dealerCardSpace5.GetComponent<Image>().sprite = cardSprites[randomCard];
            ShowUI(dealerCardSpace5);
            return;
        }
        if (dealerHand.Count == 5)
        {
            Deal(1);
            dealerCardSpace6.GetComponent<Image>().sprite = cardSprites[randomCard];
            ShowUI(dealerCardSpace6);
            return;
        }
        if (dealerHand.Count == 6)
        {
            Deal(1);
            dealerCardSpace7.GetComponent<Image>().sprite = cardSprites[randomCard];
            ShowUI(dealerCardSpace7);
            return;
        }
        if (dealerHand.Count == 7)
        {
            Deal(1);
            dealerCardSpace8.GetComponent<Image>().sprite = cardSprites[randomCard];
            ShowUI(dealerCardSpace8);
            return;
        }
    }
    public void DealerTurn()
    {
        HideUI(dealerHideCard);
        dealerHandValue = dealerHand.Sum(); //calculate sum of dealer hand
        while (dealerHandValue < 17)
        {
            ShowDealerCards();
            dealerHandValue = dealerHand.Sum(); //calculate sum of dealer hand
            dealerCardSum.text = dealerHandValue.ToString();
        }

    }

    public void Stand()
    {
        //end playing
        //gameActive = false;
        HideUI(button3);
        HideUI(button4);
        

        DealerTurn();
        if ((playerHandValue < 21 && dealerHandValue < playerHandValue) || (playerHandValue == 21 && dealerHandValue != 21) || (dealerHandValue > 21)) //if player is less than 21, dealer is less than 21, but dealer has less than player win ; if player gets to 21 and dealer did not win
        {
            //win the game
            //takeMoney = false;
            if (!onWinLose) { WinGame(); }
        }
        if (((playerHandValue < dealerHandValue) && dealerHandValue < 21) || (dealerHandValue == 21 && playerHandValue != 21)) //if player has less than dealer, and dealer has less than 21 fail ; if dealer got to 21 and player did not fail
        {
            //lose the game
            //takeMoney = false;
            if (!onWinLose) { LoseGame(); }
        }
        if ((dealerHandValue == playerHandValue)) //if both dealer and player end with same amount, draw
        {
            //tie
            //takeMoney = false;
            if (!onWinLose) { DrawGame(); }
        }

    }

    //**CLEANUP**
    public void HideAllCards()
    {
        //hide player cards
        HideUI(playerCardSpace1);
        HideUI(playerCardSpace2);
        HideUI(playerCardSpace3);
        HideUI(playerCardSpace4);
        HideUI(playerCardSpace5);
        HideUI(playerCardSpace6);
        HideUI(playerCardSpace7);
        HideUI(playerCardSpace8);

        //hide dealer cards
        HideUI(dealerCardSpace1);
        HideUI(dealerCardSpace2);
        HideUI(dealerCardSpace3);
        HideUI(dealerCardSpace4);
        HideUI(dealerCardSpace5);
        HideUI(dealerCardSpace6);
        HideUI(dealerCardSpace7);
        HideUI(dealerCardSpace8);

        HideUI(dealerHideCard);
    }
    public void StartDialogue()
    {
        dialogueText.text = dialogueLines[dialogueIndex];
    }
    public void HideDialogue()
    {
        HideUI(dialogueUI);
        dialogueIndex = 0;
    }

    //**END GAME STATES**//
    public void WinGame()
    {
        //win the game
        HideUI(dealerHideCard);
        dialogueIndex = 2;
        HideUI(betGroup);
        //HideUI(creditsPanel);
        StartDialogue();
        ShowUI(dialogueUI);
        onWinLose = true;

        dealerCardSum.text = dealerHandValue.ToString();
        //HideUI(dealerCardSumPanel);
        //HideUI(playerCardSumPanel);

        if (playerHand.Sum() == 21) { perfectScore = betAmount; } //if you got 21 you win double what you bet
        if(!takeMoney) { credits = credits + betAmount + betAmount + perfectScore; takeMoney = true; } //cost to play game given back and rewarded bet, and if you got 21 your winnings are doubled with perfect score reward
        if(takeMoney) { betAmount = 0; }
    }

    public void LoseGame()
    {
        dialogueIndex = 3;
        StartDialogue();
        //HideUI(betGroup);
        //HideUI(creditsPanel);
        ShowUI(dialogueUI);
        onWinLose = true;

        dealerCardSum.text = dealerHandValue.ToString();
        betAmount = 0;
        //No need to take away credits, it is done when you start the game
    }

    public void DrawGame()
    {
        dialogueIndex = 5;
        StartDialogue();
        //HideUI(betGroup);
        //HideUI(creditsPanel);
        ShowUI(dialogueUI);
        onWinLose = true;

        dealerCardSum.text = dealerHandValue.ToString();

        if (!takeMoney) { credits = credits + betAmount; takeMoney = true; } //get back what you wagered
        if (takeMoney) { betAmount = 0; }
    }

    public void ResetGame()
    {
        cardsDealt = 0;
        dialogueIndex = 0;
        ResetCardDeck(); //reset deck
        playerHand.Clear(); //reset player hand
        dealerHand.Clear(); //reset dealer hand
        playerHandValue = 0;
        dealerHandValue = 0;
        bust = false;
        takeMoney = false;
        onWinLose = false;
        perfectScore = 0;
        if (gameActive == false || onWinLose) { betAmount = 0; }
    }

    public void EndGame()
    {
        if (!onWinLose) { DrawGame(); } //if you left on a win/lose screen you are done, if not then cue draw
        HideUI(blackJackScreen);
        HideUI(dialogueUI);

        HideUI(creditsPanel); //credits
        HideUI(betGroup); //gambling chips
        HideUI(buttonSumReveal); //button for revealing sum
        HideUI(buttonDeal); //button for dealing

        HideUI(BetChips);
        HideUI(betAmountPanel);
        betAmount = 0;

        ResetGame();
        HideAllCards();

        HideUI(button3);
        HideUI(button4);

        HideUI(dealerCardSumPanel);
        dealerCardSum.text = "?"; //reset hiding dealer sum
        
        HideUI(playerCardSumPanel);

        isTalking = false;
        playAgain = false;
        gameActive = false;
    }


}
