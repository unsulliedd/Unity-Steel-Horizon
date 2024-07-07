using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAnyKey : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject newGameButton;

    void Update()
    {
        AnyKeyPressed();
    }

    public void AnyKeyPressed()
    {
        if (PlayerInputManager.Instance.anyKeyPerformed)
        {
            EventSystem.current.SetSelectedGameObject(newGameButton);
            titleScreen.SetActive(false);
            mainMenu.SetActive(true);
        }
    }
}
