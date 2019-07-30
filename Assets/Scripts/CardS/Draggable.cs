using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Draggable : MonoBehaviour
{
    Transform parentWhichIsHand;
    Vector2 startingCardPosition;
    CardModel cardModel;
    GameObject placeholder;
    int sortOrder;
    [HideInInspector] public bool playersChild = false;

    private void Start()
    {
        cardModel = GetComponent<CardModel>();
    }

    private void OnMouseDown()
    {
        if (!playersChild)
            return;

        startingCardPosition = transform.position;
        parentWhichIsHand = this.transform.parent;

        placeholder = new GameObject();
        placeholder.name = "PLACEHOLDER";
        placeholder.transform.SetParent(this.transform.parent);
        LayoutElement le = placeholder.AddComponent<LayoutElement>();
        placeholder.GetComponent<RectTransform>().sizeDelta = new Vector2(1.25f, 1.3333389f);
        le.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth + 1;
        le.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;

        placeholder.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

        this.transform.SetParent(this.transform.parent.parent);
        sortOrder = cardModel.GetComponent<SpriteRenderer>().sortingOrder;
        cardModel.GetComponent<SpriteRenderer>().sortingOrder = 100;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    private IEnumerator OnMouseUp()
    {
        if (cardModel.faceUp && playersChild)
        {
            this.transform.SetParent(parentWhichIsHand);
            transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
         
            Destroy(placeholder);

            yield return new WaitForEndOfFrame();
            for (int i = 0; i < parentWhichIsHand.childCount; i++)
            {
                parentWhichIsHand.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder =
                    parentWhichIsHand.transform.GetChild(i).transform.GetSiblingIndex();
            }
        }
    }

    private void OnMouseDrag()
    {
        Vector2 mousePosition = new Vector2(Input.mousePosition.x,
            Input.mousePosition.y);
        Vector2 cardPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        if (playersChild)
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
