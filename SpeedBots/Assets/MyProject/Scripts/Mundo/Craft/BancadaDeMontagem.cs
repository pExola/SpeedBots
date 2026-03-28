using UnityEngine;

public class BancadaDeMontagem : MonoBehaviour, IInteractable
{
    public void Interagir()
    {
        if (CraftingUIManager.Instance != null)
        {
            CraftingUIManager.Instance.AbrirBancada();
        }
    }
}
