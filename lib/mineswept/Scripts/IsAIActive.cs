using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IsAIActive : MonoBehaviour
{
    public Sprite active;
    public Sprite inactive;
    private AIPlayer ai;

    private Image img;
    private bool isActive;

    // Start is called before the first frame update
    void Start()
    { 
        ai = FindObjectOfType<AIPlayer>();

        img = GetComponent<Image>();
        isActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (ai.RunAI && !isActive)
        {
            Activate();
        }else if (!ai.RunAI && isActive)
        {
            Deactivate();
        }
    }

    private void Activate()
    {
        img.sprite = active;
        isActive = true;
    }

    private void Deactivate()
    {
        img.sprite = inactive;
        isActive = false;
    }

    public void BlinkActive()
    {
        if (!isActive)
        {
            img.sprite = active;
            Invoke("Deactivate", .05f);
        }
    }
}
