using FishNet;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenu : MonoBehaviour
{
    [SerializeField]
    private Button serverButton;

    [SerializeField]
    private Button clientButton;

    // Start is called before the first frame update
    void Start()
    {

        serverButton.onClick.AddListener(() =>
        {
            InstanceFinder.ServerManager.StartConnection();

            InstanceFinder.ClientManager.StartConnection();
        });

        clientButton.onClick.AddListener(() =>
        {
            InstanceFinder.ClientManager.StartConnection();
        });
    }
}
