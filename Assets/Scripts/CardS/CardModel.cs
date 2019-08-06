using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable 0649

public class CardModel : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    [SerializeField] Sprite[] faces;
    [SerializeField] Sprite cardBack;

    public int cardIndex;
    [HideInInspector] public int cardValue, cardSuit;
    public bool faceUp = false;
    [HideInInspector] public bool flipAnimation;

    //For adding arrows after check option
    [HideInInspector] public Vector3 playerPosition;
    //public Text playerName;

    //For handling cards
    [HideInInspector] public bool handling = false;
    float handlingSpeed;
    GameObject player;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public int CardValue
    {
        set { cardValue = value; }
        get { return cardValue; }
    }

    public int CardSuit
    {
        set { cardSuit = value; }
        get { return cardSuit; }
    }

    private void Update()
    {
        if (!handling)
            return;

        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, handlingSpeed * Time.deltaTime);
        if (transform.position == player.transform.position)
        {
            handling = false;
            transform.SetParent(player.transform.GetChild(1).transform.GetChild(1));
            GetComponent<SpriteRenderer>().sortingOrder = transform.parent.childCount;
        }
    }

    public void PrepareCardForHandling(GameObject player, float delayBetweenCards)
    {
        handlingSpeed = (player.transform.position - transform.position).magnitude / delayBetweenCards;
        this.player = player;
        handling = true;
    }

    public void ToggleFace(bool showFace)
    {
        if (showFace)
        {
            spriteRenderer.sprite = faces[cardIndex];
            faceUp = true;
        }
        else
        {
            spriteRenderer.sprite = cardBack;
            faceUp = false;
        }
    }

    public IEnumerator FlipACard()
    {
        flipAnimation = true;
        bool flipped = false;
        float flipDegree = 10f;

        while (!flipped)
        {
            transform.Rotate(0, flipDegree, 0);

            if (transform.eulerAngles.y < 0)
                flipped = true;

            if (transform.eulerAngles.y == 90)
            {
                ToggleFace(true);
                transform.eulerAngles = new Vector3(0, -90, 0);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        flipAnimation = false;
    }
}

#pragma warning restore 0649
