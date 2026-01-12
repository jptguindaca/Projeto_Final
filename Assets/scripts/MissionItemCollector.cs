using UnityEngine;
using TMPro;

public class MissionItemCollector : MonoBehaviour
{
    [Header("Interface")]
    public TextMeshProUGUI text_tmp;
    private int missionItemCount = 0;

    [Header("Configurações")]
    private string missionItemTag = "MissionItem";
    public int maxMissionItems = 10;

    private bool maxReached = false;
    private int eventCount = 0;

    [SerializeField] AudioSource som;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(missionItemTag))
        {
            if (missionItemCount < maxMissionItems)
            {
                missionItemCount++;

                eventCount++;
                             

                //atualiza o texto da interface
                if (text_tmp != null)
                    {
                        text_tmp.text = "Itens de Missão: " + missionItemCount + " / " + maxMissionItems;
                    }

                Destroy(other.gameObject);

                //quando atingir o máximo
                if (missionItemCount >= maxMissionItems && !maxReached)
                {
                    maxReached = true;
                    OnMaxMissionItemsReached();
                }
            }
            //Remove ou move objetos(Transform*) para uma posição
            switch (eventCount)
            {
                case 0:

                break;
                    
                case 1:
                    GameObject obj = GameObject.Find("rock02_m (30)");

                    obj.SetActive(false);

                    som.Play();
                  

                break;
            }
        }
    }

    private void OnMaxMissionItemsReached()
    {
        Debug.Log("Limite de itens de missão atingido!");
        // Aqui podes adicionar lógica: completar missão, abrir UI, tocar som, etc.
    }
}
