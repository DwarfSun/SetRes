using System.Runtime.InteropServices;

namespace SetRes;

partial class Program
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    [DllImport("user32.dll")]
    public static extern int EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

    [DllImport("user32.dll")]
    public static extern int ChangeDisplaySettingsEx(string? lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, int dwflags, IntPtr lParam);

    private const int ENUM_CURRENT_SETTINGS = -1;
    private const int CDS_UPDATEREGISTRY = 0x01;
    private const int DISP_CHANGE_SUCCESSFUL = 0;

    public static int Main(string[] args)
    {
        try
        {
            var parser = new CommandLineParser(args);
            int screenNumber = parser.GetIntValue("-s", 0);  // Default to primary screen
            int width = parser.GetIntValue("-x", 0);
            int height = parser.GetIntValue("-y", 0);

            if (width == 0 || height == 0)
            {
                ShowUsage();
                return 1;
            }

            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(devMode);

            // Get current settings
            string? displayName = screenNumber == 0 ? null : $"\\\\.\\DISPLAY{screenNumber + 1}";
            if (EnumDisplaySettings(displayName, ENUM_CURRENT_SETTINGS, ref devMode) == 0)
            {
                Console.WriteLine($"Error: Could not get current display settings for screen {screenNumber}");
                return 1;
            }

            // Update resolution
            devMode.dmPelsWidth = width;
            devMode.dmPelsHeight = height;
            devMode.dmFields = 0x00180000;  // DM_PELSWIDTH | DM_PELSHEIGHT

            int result = ChangeDisplaySettingsEx(displayName, ref devMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);

            if (result == DISP_CHANGE_SUCCESSFUL)
            {
                Console.WriteLine($"Successfully changed resolution of display {screenNumber} to {width}x{height}");
                return 0;
            }
            else
            {
                Console.WriteLine($"Failed to change resolution. Error code: {result}");
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            ShowUsage();
            return 1;
        }
    }

    static void ShowUsage()
    {
        Console.WriteLine("Usage: SetRes -s <screen_number> -x <width> -y <height>");
        Console.WriteLine("  -s: Screen number (0 for primary display, 1 for second display, etc.)");
        Console.WriteLine("  -x: Horizontal resolution");
        Console.WriteLine("  -y: Vertical resolution");
        Console.WriteLine("\nExample: SetRes -s 0 -x 1920 -y 1080");
    }
}

class CommandLineParser
{
    private readonly Dictionary<string, string> arguments;

    public CommandLineParser(string[] args)
    {
        arguments = new Dictionary<string, string>();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].StartsWith("-"))
            {
                arguments[args[i]] = args[i + 1];
            }
        }
    }    public int GetIntValue(string key, int defaultValue)
    {
        if (arguments.TryGetValue(key, out string? value) && value != null)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }
        }
        return defaultValue;
    }
}
