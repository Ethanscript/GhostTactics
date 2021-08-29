public class MainMenuView : ViewBase
{
    // Start is called before the first frame update
    public void Start()
    {
        show();
        GuideController._instance.excute(0);
    }

    // Update is called once per frame
    public void Update()
    {

    }
}
