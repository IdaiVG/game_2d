using UnityEngine;

// Efectos visuales de recompensa. Reutiliza siempre el sprite existente del
// objeto que concede los huesitos, sin añadir recursos nuevos al proyecto.
public static class BoneFeedback
{
    // Recoger un huesito: destello corto y brillante
    public static void PlayCollect(Sprite sprite, Vector3 position, int sortingOrder)
    {
        BoneFxBurst.Spawn(
            sprite,
            position,
            new Color(1f, 1f, 1f, 0.95f),
            5,
            2.6f,
            0.9f,
            0.45f,
            sortingOrder + 2);
    }

    // Romper una caja: trozos de madera hacia fuera
    public static void PlayCrateBreak(Sprite sprite, Vector3 position, int sortingOrder)
    {
        BoneFxBurst.Spawn(
            sprite,
            position,
            new Color(0.93f, 0.85f, 0.72f, 1f),
            9,
            4.2f,
            1.25f,
            0.7f,
            sortingOrder + 2);
    }

    // Araña derrotada: recompensa al morir
    public static void PlaySpiderReward(Sprite sprite, Vector3 position, int sortingOrder)
    {
        BoneFxBurst.Spawn(
            sprite,
            position,
            new Color(0.95f, 0.97f, 1f, 1f),
            10,
            4.6f,
            1.4f,
            0.75f,
            sortingOrder + 2);
    }
}
