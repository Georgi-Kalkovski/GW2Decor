using Blish_HUD.Settings;

namespace Gw2DecorBlishhudModule
{
    public static class Gw2DecorSettings
    {
        public static SettingEntry<bool> BoolSetting;
        public static SettingEntry<int> ValueRangeSetting;
        public static SettingEntry<string> StringSetting;
        public static SettingEntry<ColorType> EnumSetting;

        public static void Define(SettingCollection settings)
        {
            BoolSetting = settings.DefineSetting("boolSetting", true, "Checkbox Setting", "Boolean setting example");
            StringSetting = settings.DefineSetting("stringSetting", "defaultText", "Textbox Setting", "String setting example");
            ValueRangeSetting = settings.DefineSetting("valueRangeSetting", 20, "Slider Setting", "Int setting example");
            EnumSetting = settings.DefineSetting("enumSetting", ColorType.Blue, "Dropdown Setting", "Enum setting example");

            ValueRangeSetting.SetRange(0, 255);
        }
    }
}
