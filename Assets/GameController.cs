using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public void RestartGame()
    {
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
    }
}
