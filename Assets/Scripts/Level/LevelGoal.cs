using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelGoal : MonoBehaviour
{
    [Header("Interfaz de victoria")]
    public GameObject victoryPanel;
    public Text bonesResultLabel;
    public string bonesResultFormat = "HUESITOS CONSEGUIDOS: {0}\nPUNTUACIÓN TOTAL: {0}";

    [Header("Recompensa")]
    public int boneReward = 25;

    [Header("Escenas")]
    public string mainMenuScene = "MainMenu";

    private bool completed;

    void Start()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        Complete(player);
    }

    public void Complete(PlayerController player)
    {
        if (completed)
            return;

        // Un jugador muerto no puede completar el nivel ni cobrar la recompensa
        if (player != null && player.IsDead)
            return;

        completed = true;

        // Recompensa por completar el nivel: una sola vez por nivel
        BoneScoreManager.AddBonesOnce(
            "level:complete:" + gameObject.scene.name,
            boneReward);

        if (player != null)
            player.CompleteLevel();

        StartCoroutine(ShowVictory());
    }

    IEnumerator ShowVictory()
    {
        yield return new WaitForSeconds(0.35f);

        if (bonesResultLabel != null)
            bonesResultLabel.text = string.Format(bonesResultFormat, BoneScoreManager.TotalBones);

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    public void RestartLevel()
    {
        LevelRestarter.RestartCurrentLevel();
    }

    public void GoToMainMenu()
    {
        // Salir tambien deja el contador en cero para la siguiente partida
        LevelRestarter.LoadScene(mainMenuScene);
    }
}
