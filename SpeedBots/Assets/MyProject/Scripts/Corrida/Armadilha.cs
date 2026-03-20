using UnityEngine;

public class Armadilha : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. O primeiro aviso
        Debug.Log($"[ARMADILHA] Algo pisou em mim! Nome: {collision.gameObject.name}");

        SpeedBotMovment player = collision.GetComponent<SpeedBotMovment>();
        SpeedBotIA ia = collision.GetComponent<SpeedBotIA>();

        if (player != null)
        {
            Debug.Log("[ARMADILHA] Acertei o Player! Aplicando Stun e me destruindo...");
            player.TomarStunDeItem(1.5f);
            Destroy(gameObject);
        }
        else if (ia != null)
        {
            Debug.Log("[ARMADILHA] Acertei a IA! Aplicando Stun e me destruindo...");
            ia.TomarStunDeItem(1.5f);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"[ARMADILHA] O objeto {collision.gameObject.name} ignorado. Não tem os scripts de movimento dos robôs.");
        }
    }
}
