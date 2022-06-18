using UnityEngine;
using UnityEngine.UI;

public class SnapTurnToggle : MonoBehaviour
{
    public GameObject continuousTurnGameObject, snapTurnGameObject;

    private void Awake()
    {
        GetComponent<Toggle>().onValueChanged.AddListener(Toggle);
    }

    private void Toggle(bool enableSnap)
    {
        continuousTurnGameObject.SetActive(!enableSnap);
        snapTurnGameObject.SetActive(enableSnap);
    }
}
