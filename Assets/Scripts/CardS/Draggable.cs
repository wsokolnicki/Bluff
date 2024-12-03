using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Draggable : MonoBehaviour
{
    private Transform parentWhichIsHand = null;
    private Vector2 startingCardPosition = new Vector2(0,0);
    private CardModel cardModel = null;
    private SpriteRenderer cardSpriteRenderer = null;
    private GameObject placeholder = null;
    private int sortOrder = 0;
    [HideInInspector] public bool PlayersChild = false;

    [SerializeField] private Vector2 placeholderCardSize = new Vector2(1f,1f);

    private void Start()
    {
        cardModel = GetComponent<CardModel>();
        cardSpriteRenderer = cardModel.CardSpriteRenderer;
    }

    private void OnMouseDown()
    {
        if (!PlayersChild)
        {
            return;
        }

        SpriteRenderer _cardModelSpriteRenderer = cardModel.GetComponent<SpriteRenderer>();

        startingCardPosition = transform.position;
        parentWhichIsHand = this.transform.parent;

        placeholder = new GameObject();
        placeholder.name = "PLACEHOLDER";
        placeholder.transform.SetParent(parentWhichIsHand);
        LayoutElement _le = placeholder.AddComponent<LayoutElement>();
        placeholder.GetComponent<RectTransform>().sizeDelta = placeholderCardSize;
        _le.preferredWidth = _le.preferredWidth + 1;
        _le.preferredHeight = _le.preferredHeight;
        _le.flexibleWidth = 0;
        _le.flexibleHeight = 0;

        placeholder.transform.SetSiblingIndex(this.transform.GetSiblingIndex());
        this.transform.SetParent(parentWhichIsHand.parent);
        sortOrder = _cardModelSpriteRenderer.sortingOrder;
        sortOrder = 100;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    private IEnumerator OnMouseUp()
    {
        if (cardModel.FaceUp && PlayersChild)
        {
            this.transform.SetParent(parentWhichIsHand);
            transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
         
            Destroy(placeholder);

            yield return new WaitForEndOfFrame();
            for (int i = 0; i < parentWhichIsHand.childCount; i++)
            {
                parentWhichIsHand.transform.GetChild(i).GetComponent<CardModel>().CardSpriteRenderer.sortingOrder =
                    parentWhichIsHand.transform.GetChild(i).transform.GetSiblingIndex();
            }
        }
    }

    private void OnMouseDrag()
    {
        Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 cardPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        if (PlayersChild)
        {
            transform.position = cardPosition;

            for (int i = 0; i < parentWhichIsHand.childCount; i++)
            {
                if (this.transform.position.x < parentWhichIsHand.GetChild(i).transform.position.x)
                {
                    placeholder.transform.SetSiblingIndex(i);
                    break;
                }
            }
        }
    }
}
