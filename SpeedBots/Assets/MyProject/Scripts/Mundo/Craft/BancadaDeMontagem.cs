using UnityEngine;

public class BancadaDeMontagem : MonoBehaviour, IInteractable
{
    public void Interagir()
    {
        Debug.Log("[BANCADA] Acessando o terminal da Oficina...");

        if (OficinaUIManager.Instance != null)
        {
            OficinaUIManager.Instance.AbrirOficina();
        }
    }
}
