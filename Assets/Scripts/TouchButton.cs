using UnityEngine;
using UnityEngine.EventSystems;

public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ButtonType
    {
        Left,
        Right,
        Jump,
        Attack
    }

    public ButtonType type;
    public TrexController player;

    public void OnPointerDown(PointerEventData eventData)
    {
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
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (type == ButtonType.Left || type == ButtonType.Right)
            player.MoveHorizontal(0f);
    }
}