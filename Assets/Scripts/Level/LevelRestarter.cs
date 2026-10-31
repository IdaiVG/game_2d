using UnityEngine.SceneManagement;

// Punto unico de reinicio del juego.
// Cualquier camino de reinicio (muerte, tecla R, boton REINICIAR, salida al
// menu) pasa por aqui para que el puntaje y el registro de recompensas se
// limpien SIEMPRE antes de recargar la escena. No es un administrador de
// puntaje: solo ordena el reinicio usando el existente BoneScoreManager.
public static class LevelRestarter
{
    // Recarga el nivel activo empezando de cero (muerte, R, REINICIAR)
    public static void RestartCurrentLevel()
    {
        ResetScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Carga otra escena empezando de cero (volver al menu principal)
    public static void LoadScene(string sceneName)
    {
        ResetScore();
        SceneManager.LoadScene(sceneName);
    }

    // Limpia TotalBones y todas las claves de AddBonesOnce
    static void ResetScore()
    {
        BoneScoreManager.ResetScore();
    }
}
