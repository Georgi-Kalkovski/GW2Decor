namespace DecorBlishhudModule.CustomTabLogic
{
    public interface ICustomTabOwner
    {
        CustomTabCollection TabsGroup1 { get; }
        CustomTabCollection TabsGroup2 { get; }

        CustomTab SelectedTabGroup1 { get; set; }
        CustomTab SelectedTabGroup2 { get; set; }
    }
}
