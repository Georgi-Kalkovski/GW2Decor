namespace DecorBlishhudModule.CustomControls.CustomTab
{
    public interface ICustomTabOwner
    {
        CustomTabCollection TabsGroup1 { get; }
        CustomTabCollection TabsGroup2 { get; }
        CustomTabCollection TabsGroup3 { get; }

        CustomTab SelectedTabGroup1 { get; set; }
        CustomTab SelectedTabGroup2 { get; set; }
        CustomTab SelectedTabGroup3 { get; set; }
    }
}
