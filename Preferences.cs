using MelonLoader;
using MelonLoader.Preferences;
using System.Text.RegularExpressions;

namespace GrassEnabler
{
    public abstract class ValueValidator<T>
    {
        public abstract bool IsValid(object value);
        public abstract object EnsureValid(object value);
    }

    public class HexColorValidator : ValueValidator
    {
        private static readonly Regex hexRegex = new Regex("^[0-9a-fA-F]+$", RegexOptions.Compiled);
        private static readonly HashSet<int> validLengths = new HashSet<int> { 3, 4, 6, 8 };

        public override bool IsValid(object value) { return value is string s && IsValidHex(s); }

        public override object EnsureValid(object value)
        {
            if (value is not string s || !IsValidHex(s)) { return "#C0B154"; }
            return NormalizeToStandard(s);
        }

        private static string NormalizeToStandard(string input)
        {
            var hex = Normalize(input);
            if (hex.Length == 3 || hex.Length == 4) { hex = string.Concat(hex.Select(c => $"{c}{c}")); }
            return "#" + hex.ToUpper();
        }

        private static bool IsValidHex(string input)
        {
            var normalized = Normalize(input);
            if (string.IsNullOrEmpty(normalized) || !validLengths.Contains(normalized.Length)) { return false; }
            return hexRegex.IsMatch(normalized);
        }

        private static string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input)) { return null; }
            input = input.Trim();
            if (input.StartsWith("#")) { input = input.Substring(1); }
            else if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) { input = input.Substring(2); }
            return input;
        }
    }

    public class Preferences
	{
		private const string CONFIG_FILE = "config.cfg";
		private const string USER_DATA = "UserData/GrassEnabler/";
        internal static Dictionary<MelonPreferences_Entry, object> LastSavedValues = new();

        internal static MelonPreferences_Category GrassCountCategory;
        internal static MelonPreferences_Entry<bool> PrefEnabled;
        internal static MelonPreferences_Entry<int> PrefRingCount;
        internal static MelonPreferences_Entry<int> PrefPitCount;
        internal static MelonPreferences_Entry<int> PrefUpperParkCount;
        internal static MelonPreferences_Entry<int> PrefLowerParkCount;
        internal static MelonPreferences_Entry<int> PrefFlatLandCount;

        internal static MelonPreferences_Category GrassSettingsCategory;
        internal static MelonPreferences_Entry<float> PrefHeight;
        internal static MelonPreferences_Entry<float> PrefWidth;
        internal static MelonPreferences_Entry<string> PrefColor;
        internal static MelonPreferences_Entry<bool> PrefRandomColor;
		
        internal static MelonPreferences_Category GrassVisualsCategory;
        internal static MelonPreferences_Entry<bool> PrefRemoval;
        internal static MelonPreferences_Entry<bool> PrefRegrow;
        internal static MelonPreferences_Entry<float> PrefTimeBeforeRegrow;
        internal static MelonPreferences_Entry<float> PrefGrowthTime;

        internal static void InitPrefs()
		{
			if (!Directory.Exists(USER_DATA)) { Directory.CreateDirectory(USER_DATA); }

            //Grass Count Settings
            GrassCountCategory = MelonPreferences.CreateCategory("GrassCount", "Grass Count");
            GrassCountCategory.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            PrefEnabled = GrassCountCategory.CreateEntry("Enabled", true, "Enabled", "Toggles Grass On/Off");
            PrefRingCount = GrassCountCategory.CreateEntry("RingCount", 5000, "Ring Count", "Set the number of Grass in the Ring", validator: new ValueRange<int>(0, int.MaxValue));
            PrefPitCount = GrassCountCategory.CreateEntry("PitCount", 5000, "Pit Count", "Set the number of Grass in the Pit", validator: new ValueRange<int>(0, int.MaxValue));
            PrefUpperParkCount = GrassCountCategory.CreateEntry("UpperParkCount", 5000, "Upper Park Count", "Set the number of Grass in the Upper Park", validator: new ValueRange<int>(0, int.MaxValue));
            PrefLowerParkCount = GrassCountCategory.CreateEntry("LowerParkCount", 5000, "Lower Park Count", "Set the number of Grass in the Lower Park", validator: new ValueRange<int>(0, int.MaxValue));
            if (Main.flatLandModFound) { PrefFlatLandCount = GrassCountCategory.CreateEntry("FlatLandCount", 5000, "FlatLand Count", "Set the number of Grass in FlatLand", validator: new ValueRange<int>(0, int.MaxValue)); }
            //Grass General settings
            GrassSettingsCategory = MelonPreferences.CreateCategory("GrassSettings", "Grass Settings");
            GrassSettingsCategory.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            PrefHeight = GrassSettingsCategory.CreateEntry("GrassHeight", 0.5f, "Grass Height", "Changes Grass Height. Default: 0.5", validator: new ValueRange<float>(0f, float.MaxValue));
            PrefWidth = GrassSettingsCategory.CreateEntry("GrassWidth", 0.5f, "Grass Width", "Changes Grass Width. Default: 0.5", validator: new ValueRange<float>(0f, float.MaxValue));
            PrefColor = GrassSettingsCategory.CreateEntry("GrassColor", "C0B154", "Grass Color", "Sets the Color of the Grass. Supports (HexCode), 0x(HexCode), and #(HexCode) style inputs. Alpha included (jank). Default: C0B154", validator: new HexColorValidator());
            PrefRandomColor = GrassSettingsCategory.CreateEntry("RandomColoredGrass", false, "Random Colored Grass", "Recolors each Grass to a Random Color. Default: Off");

            //Grass Visuals settings
            GrassVisualsCategory = MelonPreferences.CreateCategory("GrassVisuals", "GrassVisuals");
            GrassVisualsCategory.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            PrefRemoval = GrassVisualsCategory.CreateEntry("GrassRemoval", true, "Grass Removal", "Removes the Grass with Spawned and Grounded Structures. Default: On");
            PrefRegrow = GrassVisualsCategory.CreateEntry("RegrowGrass", true, "Regrow Grass", "Regrows the Grass after it is Destroyed. Default: On");
            PrefTimeBeforeRegrow = GrassVisualsCategory.CreateEntry("TimeBeforeRegrowth", 5f, "Time Before Regrowth", "Controls how many Seconds till Grass starts Regrowing. Default: 5", validator: new ValueRange<float>(0f, float.MaxValue));
            PrefGrowthTime = GrassVisualsCategory.CreateEntry("GrowthTime", 10f, "Growth Time", "Controls how many Seconds till Grass is Fully Grown. Default: 10", validator: new ValueRange<float>(0f, float.MaxValue));

            StoreLastSavedPrefs();
		}

		internal static void StoreLastSavedPrefs()
		{
			List<MelonPreferences_Entry> prefs = new();
			prefs.AddRange(GrassSettingsCategory.Entries);
			prefs.AddRange(GrassCountCategory.Entries);
			prefs.AddRange(GrassVisualsCategory.Entries);

			foreach (MelonPreferences_Entry entry in  prefs) { LastSavedValues[entry] = entry.BoxedValue; }
		}

		/*public static bool AnyPrefsChanged()
		{
			foreach (KeyValuePair<MelonPreferences_Entry, object> pair in LastSavedValues)
			{
				if (!pair.Key.BoxedValue.Equals(pair.Value)) { return true; }
			}
			return false;
		}*/

		public static bool IsPrefChanged(MelonPreferences_Entry entry)
		{
			if (LastSavedValues.TryGetValue(entry, out object? lastValue)) { return !entry.BoxedValue.Equals(lastValue); }
			return false;
		}
	}
}