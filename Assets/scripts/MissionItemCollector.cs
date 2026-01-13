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
            switch (eventCount)
            {
                     case 0:

                     break;
                    
                     case 1:
                    GameObject light = GameObject.Find("Luz1");
                    GameObject light1 = GameObject.Find("Luz2");
                    GameObject light2 = GameObject.Find("Luz3");
                    GameObject light3 = GameObject.Find("Luz4");
                    GameObject light4 = GameObject.Find("Luz5");
                     GameObject light5 = GameObject.Find("CarvedPost2 (1)");

                    light.SetActive(false);
                    light1.SetActive(false);
                    light2.SetActive(false);
                    light3.SetActive(false);
                    light4.SetActive(false);

                    light5.transform.localRotation = Quaternion.Euler(78f, -100f, 100f);

                    som.Play();
                  
                     break;

                    case 2:
                     GameObject obj2 = GameObject.Find("rock02_m (30)");

                    obj2.SetActive(false);

                    som.Play();
                     break;

                     case 3:
                    GameObject obj3 = GameObject.Find("");


                    som.Play();
                     break;

                    case 4:
                    GameObject obj4 = GameObject.Find("");


                    som.Play();
                    break;

                    case 5:

                    GameObject obj5 = GameObject.Find("");

                   

                    som.Play();

                    break;

                    case 6:

                    GameObject obj6 = GameObject.Find("");


                    som.Play();

                    break;

                    case 7:
                    
                   GameObject obj7 = GameObject.Find("");

                    som.Play();

                    break;

            }
        }
    }

    private void OnMaxMissionItemsReached()
    {
        Debug.Log("Limite de itens de missão atingido!");
    }
}
