using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LoadSceneTrigger : MonoBehaviour
{
    [SerializeField] string sceneName;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(LoadScene);            
    }

    void LoadScene()
    {
        LoL.State.LoadSceneSetReturn(sceneName);
    }
}
