using System.Collections;
using UnityEngine;

public class CardModel : MonoBehaviour
{
    private SpriteRenderer spriteRenderer = null;

    [SerializeField] private Sprite[] faces = new Sprite[24];
    [SerializeField] private Sprite cardBack = null;

    public int CardIndex = 0;
    private int cardValue, cardSuit = 0;
    public bool FaceUp = false;
    [HideInInspector] public bool flipAnimation = false;

    //For adding arrows after check option
    [HideInInspector] public Vector3 PlayerPosition = Vector3.zero;
    //public Text playerName;

    //For handling cards
    [HideInInspector] public bool Handling = false;
    private float handlingSpeed = 0f;
    private GameObject player = null;
    
    [HideInInspector] public Transform CardCopyInPlayerHand = null;

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
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!Handling)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, handlingSpeed * Time.deltaTime);

        if (transform.position == player.transform.position)
        {
            Handling = false;
            transform.SetParent(player.transform.GetChild(1).transform.GetChild(1));
            GetComponent<SpriteRenderer>().sortingOrder = transform.parent.childCount;
        }
    }

    public void PrepareCardForHandling(GameObject player, float delayBetweenCards)
    {
        handlingSpeed = (player.transform.position - transform.position).magnitude / delayBetweenCards;
        this.player = player;
        Handling = true;
    }

    public void ToggleFace(bool showFace)
    {
        if (showFace)
        {
            spriteRenderer.sprite = faces[CardIndex];
            FaceUp = true;
        }
        else
        {
            spriteRenderer.sprite = cardBack;
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
            transform.Rotate(0, flipDegree, 0);

            if (transform.eulerAngles.y < 0)
            {
                flipped = true;
            }

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