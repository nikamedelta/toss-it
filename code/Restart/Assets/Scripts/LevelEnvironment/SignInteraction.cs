using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class SignInteraction : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrompt = null;
    private GameController gameController;
    [SerializeField] public String text;

    private void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        if (gameController == null)
            throw new Exception("GameController could not be found! Make sure the GO is named GameController!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.tag.Equals("Player")) return;
        if (buttonPrompt != null) buttonPrompt.SetActive(true);

        gameController.currentSign = this;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.tag.Equals("Player")) return;
        if (buttonPrompt != null) buttonPrompt.SetActive(false);
        
        gameController.currentSign = null;
    }
}
