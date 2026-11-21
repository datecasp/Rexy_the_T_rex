using UnityEngine;
using UnityEngine.EventSystems;

public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ButtonType
    {
        Left,
        Right,
        Jump,
        Attack,
        Run
    }

    private void Awake()
    {
        if (player == null)
            player = FindFirstObjectByType<TrexController>();
    }

    public ButtonType type;
    public TrexController player;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (player == null)
            return;

        switch (type)
        {
            case ButtonType.Left:
                player.MoveHorizontal(-1f);
                break;
            case ButtonType.Right:
                player.MoveHorizontal(1f);
                break;
            case ButtonType.Jump:
                player.Jump();
                break;
            case ButtonType.Attack:
                player.Attack();
                break;
            case ButtonType.Run:
                player.SetRunning(true);
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (player == null)
            return;

        if (type == ButtonType.Left || type == ButtonType.Right)
            player.MoveHorizontal(0f);

        if (type == ButtonType.Run)
            player.SetRunning(false);
    }
}