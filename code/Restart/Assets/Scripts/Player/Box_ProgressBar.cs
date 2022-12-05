using UnityEngine;
using UnityEngine.UI;

public class Box_ProgressBar : MonoBehaviour
{
    private enum Status
    {
        FALLING, RELOADING, NONE
    }
    [SerializeField] private GameObject progressBar;
    [SerializeField] private Image progressBar_fill;
    public float fallingTime = 0;
    public float reloadTime = 0;

    private Status status = Status.NONE;
    private float currentTime = 0;

    private Color reloadColor;
    
    public void StartProgressBar(Color fallingColor, float fallingTime, Color reloadColor, float reloadTime)
    {
        currentTime = 0;
        this.reloadColor = reloadColor;
        this.fallingTime = fallingTime;
        this.reloadTime = reloadTime;
        
        progressBar.SetActive(true);
        progressBar_fill.color = fallingColor;
        status = Status.FALLING;
    }

    public void InterruptBar()
    {
        // cancel bar and go to reload stage immediately 
        if (status == Status.FALLING)
        {
            currentTime = 0;
            status = Status.RELOADING;
            progressBar_fill.color = reloadColor;
        }
        
    }

    private void Update()
    {
        switch (status)
        {
            case Status.FALLING:
            {
                currentTime += Time.deltaTime;
                if (currentTime > fallingTime)
                {
                    currentTime = fallingTime;
                    status = Status.RELOADING;
                    progressBar_fill.color = reloadColor;

                    progressBar_fill.fillAmount = 1;
                    currentTime = 0;
                    return;
                }
                float ratio = currentTime / fallingTime;
                progressBar_fill.fillAmount = ratio;
                
                break;
            }
            case Status.RELOADING:
            {
                currentTime += Time.deltaTime;
                if (currentTime > reloadTime)
                {
                    currentTime = reloadTime;
                    status = Status.NONE;
                    progressBar.SetActive(false);
                    
                    return;
                }
                float ratio = 1-(currentTime / reloadTime);
                progressBar_fill.fillAmount = ratio;
                break;
            }
        }
    }
}
