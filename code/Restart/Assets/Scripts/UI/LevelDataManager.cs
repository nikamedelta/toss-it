using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LevelDataManager : MonoBehaviour
    {
        [SerializeField] private Sprite filledJar;

        [Header("Level1 Images")] 
        [SerializeField] private Image jar1_1;
        [SerializeField] private Image jar1_2;
        [SerializeField] private Image jar1_3;
        
        [Header("Level2 Images")] 
        [SerializeField] private Image jar2_1;
        [SerializeField] private Image jar2_2;
        [SerializeField] private Image jar2_3;
        
        [Header("Level3 Images")] 
        [SerializeField] private Image jar3_1;
        [SerializeField] private Image jar3_2;
        [SerializeField] private Image jar3_3;

        /// <summary>
        /// Save the data of collected jars after finishing a level.
        /// </summary>
        public void SaveLevelData(int level, Objective[] objectives)
        {
            String newData = "";
            String data = PlayerPrefs.GetString("level" + level, "000");

            for(int i = 0; i<objectives.Length; i++)
            {
                Objective objective = objectives[i];
                if (objective.reached||data[i]=='1') newData += "1";
                else newData += "0";
            }
            PlayerPrefs.SetString("level"+level, newData);
        }

        /// <summary>
        /// Apply saved level data in level selection UI
        /// </summary>
        public void ApplySavedData()
        {
            String level1 = PlayerPrefs.GetString("level1", "000");
            String level2 = PlayerPrefs.GetString("level2", "000");
            String level3 = PlayerPrefs.GetString("level3", "000");
            
            // set level1 jars
            if (level1[0] == '1') jar1_1.sprite = filledJar;
            if (level1[1] == '1') jar1_2.sprite = filledJar;
            if (level1[2] == '1') jar1_3.sprite = filledJar;
            
            // set level2 jars
            if (level2[0] == '1') jar2_1.sprite = filledJar;
            if (level2[1] == '1') jar2_2.sprite = filledJar;
            if (level2[2] == '1') jar2_3.sprite = filledJar;
            
            // set level1 jars
            if (level3[0] == '1') jar3_1.sprite = filledJar;
            if (level3[1] == '1') jar3_2.sprite = filledJar;
            if (level3[2] == '1') jar3_3.sprite = filledJar;
            
            Debug.Log("level1: " + level1);
            Debug.Log("level2: " + level2);
            Debug.Log("level3: " + level3);
        }
        
    }
}
