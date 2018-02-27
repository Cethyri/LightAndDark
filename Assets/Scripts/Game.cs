using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] GameObject m_pause;
    [SerializeField] GameObject m_HUD;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !m_pause.activeSelf)
        {
            Pause();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && m_pause.activeSelf)
        {
            Unpause();
        }
    }

    private void Pause()
    {
        m_pause.SetActive(true);
        m_HUD.SetActive(false);
        Time.timeScale = 0.0f;
    }

    public void Unpause()
    {
        m_pause.SetActive(false);
        m_HUD.SetActive(true);
        Time.timeScale = 1.0f;
    }
}
