using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class LevelController : MonoBehaviour
{
    [SerializeField] public Objective[] objectives;
    [SerializeField] private Image[] objectiveUI;
    [SerializeField] private Image[] objectivesOnFinish;
    [SerializeField] private Sprite jarFull;

    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI timeOnFinishText;
    [SerializeField] private GameObject goalPrompt;
    private float time = 0;
    
    private int reachedObjectives = 0;
    private GameController gameController;

    [SerializeField]
    public int currentLevel = 0;
    
    private void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        if (gameController == null) throw new Exception("GameController not found");
    }

    public void UpdateObjectives(int index)
    {
        Debug.Log("objective reached");
        reachedObjectives++;
        objectiveUI[index].sprite = jarFull;
        objectivesOnFinish[index].sprite = jarFull;

        if (reachedObjectives == objectives.Length)
        {
            ReachedAllObjectives();
        }
    }

    private void ReachedAllObjectives()
    {
        Debug.Log("all objectives have been reached");
    }

    private void Update()
    {
        if (Time.timeScale != 0) time += Time.unscaledDeltaTime;
        // update displayed time
        float displayedTime = Mathf.FloorToInt(time);
        String seconds = displayedTime % 60 >= 10 ? (displayedTime % 60).ToString() : "0" + (displayedTime % 60);
        String minutes = Mathf.FloorToInt(displayedTime/60).ToString();
        if (minutes.Length == 1) minutes = "0" + minutes;
        timeText.text = "TIME " + minutes + ":" + seconds;
        timeOnFinishText.text = minutes + ":" + seconds;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.tag.Equals("Player")) return;
        if (goalPrompt != null) goalPrompt.SetActive(true);
        gameController.inGoal = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.tag.Equals("Player")) return;
        if (goalPrompt != null) goalPrompt.SetActive(false);
        gameController.inGoal = false;
    }
}
