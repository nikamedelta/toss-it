using UnityEngine;

public class CreditsScroll : MonoBehaviour
{
    [SerializeField] private RectTransform credits;
    [SerializeField] private RectTransform startPosition; 
    
    private float speed = .4f;
    private bool scrolling = false;
    public void ActivateCredits()
    {
        scrolling = true;
    }

    public void StopCredits()
    {
        credits.anchoredPosition = startPosition.anchoredPosition;
        scrolling = false;
    }

    private void Update()
    {
        if (scrolling)
        {
            Vector2 temp = credits.anchoredPosition;
            credits.anchoredPosition = temp + Vector2.up*speed;
        }
    }
}