using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class RightClick : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onClick;
    public bool interactable;

    public void Awake()
    {
        interactable = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (interactable && eventData.button == PointerEventData.InputButton.Right)
        {
            onClick.Invoke();
        }
    }

}
