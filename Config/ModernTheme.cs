using System.Drawing;

namespace SpinARayan.Config
{
    /// <summary>
    /// Modern color theme for the application with blue accent
    /// Primary: Dark Navy, Secondary: Light Blue Grey, Accent: Modern Blue
    /// </summary>
    public static class ModernTheme
    {
        // Primary Colors - Main UI elements
        public static readonly Color PrimaryDark = Color.FromArgb(26, 35, 126);      // Dark Navy #1A237E
        public static readonly Color PrimaryMedium = Color.FromArgb(48, 63, 159);    // Medium Navy #303F9F
        public static readonly Color PrimaryLight = Color.FromArgb(92, 107, 192);    // Light Navy #5C6BC0
        
        // Secondary Colors - Supporting elements
        public static readonly Color SecondaryDark = Color.FromArgb(144, 164, 174);  // Dark Blue Grey #90A4AE
        public static readonly Color SecondaryMedium = Color.FromArgb(176, 190, 197); // Medium Blue Grey #B0BEC5
        public static readonly Color SecondaryLight = Color.FromArgb(207, 216, 220);  // Light Blue Grey #CFD8DC
        
        // Accent Colors - Interactive elements
        public static readonly Color AccentBlue = Color.FromArgb(33, 150, 243);      // Modern Blue #2196F3
        public static readonly Color AccentBlueLight = Color.FromArgb(66, 165, 245); // Light Blue #42A5F5
        public static readonly Color AccentBlueDark = Color.FromArgb(25, 118, 210);  // Dark Blue #1976D2
        
        // Background Colors
        public static readonly Color BackgroundDark = Color.FromArgb(18, 18, 18);    // Rich Dark #121212
        public static readonly Color BackgroundElevated = Color.FromArgb(30, 30, 30); // Elevated Dark #1E1E1E
        public static readonly Color BackgroundPanel = Color.FromArgb(38, 38, 38);   // Panel Dark #262626
        
        // Status Colors
        public static readonly Color Success = Color.FromArgb(76, 175, 80);          // Green #4CAF50
        public static readonly Color Warning = Color.FromArgb(255, 193, 7);          // Amber #FFC107
        public static readonly Color Error = Color.FromArgb(244, 67, 54);            // Red #F44336
        public static readonly Color Info = Color.FromArgb(3, 169, 244);             // Light Blue #03A9F4
        
        // Text Colors
        public static readonly Color TextPrimary = Color.FromArgb(255, 255, 255);    // White
        public static readonly Color TextSecondary = Color.FromArgb(189, 189, 189);  // Light Grey #BDBDBD
        public static readonly Color TextDisabled = Color.FromArgb(117, 117, 117);   // Grey #757575
        
        // Special Colors for game elements
        public static readonly Color Money = Color.FromArgb(76, 175, 80);            // Green for money
        public static readonly Color Gems = Color.FromArgb(33, 150, 243);            // Blue for gems
        public static readonly Color Luck = Color.FromArgb(255, 193, 7);             // Amber for luck
        public static readonly Color Rebirth = Color.FromArgb(156, 39, 176);         // Purple for rebirth
        public static readonly Color Rare = Color.FromArgb(255, 152, 0);             // Orange for rare items
        
        // Border Colors
        public static readonly Color BorderDark = Color.FromArgb(66, 66, 66);        // Dark border #424242
        public static readonly Color BorderLight = Color.FromArgb(97, 97, 97);       // Light border #616161
        public static readonly Color BorderAccent = Color.FromArgb(33, 150, 243);    // Accent border
    }
}
