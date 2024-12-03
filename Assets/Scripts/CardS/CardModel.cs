using System.Collections;
using UnityEngine;

public class CardModel : MonoBehaviour
{
    [SerializeField] private Sprite[] faces = new Sprite[24];
    [SerializeField] private Sprite cardBack = null;

    public int CardIndex = 0;

    [HideInInspector] public bool FaceUp = false;
    [HideInInspector] public bool flipAnimation = false;
    [HideInInspector] public Transform CardCopyInPlayerHand = null;
    [HideInInspector] public SpriteRenderer CardSpriteRenderer = null;

    //For adding arrows after check option
    [HideInInspector] public Vector3 PlayerPosition = Vector3.zero;

    //For handling cards
    [HideInInspector] public bool Handling = false;
    private float handlingSpeed = 0f;
    [HideInInspector] public GameObject CardOwner = null;

    private int cardValue, cardSuit = 0;

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

    void Awake()
    {
        CardSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (!Handling)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, CardOwner.transform.position, handlingSpeed * Time.deltaTime);

        if (transform.position == CardOwner.transform.position)
        {
            Handling = false;
            transform.SetParent(CardOwner.transform.GetChild(1).transform.GetChild(1));
            CardSpriteRenderer.sortingOrder = transform.parent.childCount;
        }
    }

    public void PrepareCardForHandling(GameObject player, float delayBetweenCards)
    {
        handlingSpeed = (player.transform.position - transform.position).magnitude / delayBetweenCards;
        this.CardOwner = player;
        PlayerPosition = player.transform.position;
        Handling = true;
    }

    public void ToggleFace(bool showFace)
    {
        if (showFace)
        {
            CardSpriteRenderer.sprite = faces[CardIndex];
            FaceUp = true;
        }
        else
        {
            CardSpriteRenderer.sprite = cardBack;
            FaceUp = false;
        }
    }

    public IEnumerator FlipACard()
    {
        flipAnimation = true;
        bool flipped = false;
        float flipDegree = 10f;

        while (!flipped)
        {
            transform.Rotate(0f, flipDegree, 0f);

            if (transform.eulerAngles.y < 0f)
            {
                flipped = true;
            }

            if (transform.eulerAngles.y == 90f)
            {
                ToggleFace(true);
                transform.eulerAngles = new Vector3(0f, -90f, 0f);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        flipAnimation = false;
    }
}