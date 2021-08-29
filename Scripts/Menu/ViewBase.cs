using UnityEngine;

public class ViewBase : MonoBehaviour
{
    public virtual void show()
    {
        gameObject.SetActive(true);
    }

    public virtual void hide()
    {
        gameObject.SetActive(false);
    }
}
