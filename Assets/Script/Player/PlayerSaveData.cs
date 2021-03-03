using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlayerStats
{
    public int score = 0;
    public int vitality;
    public int strength;
    public int speed;
    public int luck;
    public int defense;
    public Vector2 position;
    public string sceneName;
}
