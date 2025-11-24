namespace Vigilance.Logging;

public static class Ansi
{
    public const string Esc = "\e[";
    public const string Reset = $"{Esc}0m";

    public static class Style
    {
        public const string Bold = $"{Esc}1m";
        public const string Dim = $"{Esc}2m";
        public const string Italic = $"{Esc}3m";
        public const string Underline = $"{Esc}4m";
        public const string Blink = $"{Esc}5m";
        public const string Inverse = $"{Esc}7m";
        public const string Hidden = $"{Esc}8m";
        public const string Strikethrough = $"{Esc}9m";

        public const string ResetBold = $"{Esc}22m";
        public const string ResetDim = $"{Esc}22m";
        public const string ResetItalic = $"{Esc}23m";
        public const string ResetUnderline = $"{Esc}24m";
        public const string ResetBlink = $"{Esc}25m";
        public const string ResetInverse = $"{Esc}27m";
        public const string ResetHidden = $"{Esc}28m";
        public const string ResetStrikethrough = $"{Esc}29m";
    }

    public static class Foreground
    {
        public const string Black = $"{Esc}30m";
        public const string Red = $"{Esc}31m";
        public const string Green = $"{Esc}32m";
        public const string Yellow = $"{Esc}33m";
        public const string Blue = $"{Esc}34m";
        public const string Magenta = $"{Esc}35m";
        public const string Cyan = $"{Esc}36m";
        public const string White = $"{Esc}37m";
        public const string Default = $"{Esc}39m";

        public const string BrightBlack = $"{Esc}90m";
        public const string BrightRed = $"{Esc}91m";
        public const string BrightGreen = $"{Esc}92m";
        public const string BrightYellow = $"{Esc}93m";
        public const string BrightBlue = $"{Esc}94m";
        public const string BrightMagenta = $"{Esc}95m";
        public const string BrightCyan = $"{Esc}96m";
        public const string BrightWhite = $"{Esc}97m";

        public static string Id(byte id)
        {
            return $"{Esc}38;5;{id}m";
        }

        public static string Rgb(byte r, byte g, byte b)
        {
            return $"{Esc}38;2;{r};{g};{b}m";
        }
    }

    public static class Background
    {
        public const string Black = $"{Esc}40m";
        public const string Red = $"{Esc}41m";
        public const string Green = $"{Esc}42m";
        public const string Yellow = $"{Esc}43m";
        public const string Blue = $"{Esc}44m";
        public const string Magenta = $"{Esc}45m";
        public const string Cyan = $"{Esc}46m";
        public const string White = $"{Esc}47m";
        public const string Default = $"{Esc}49m";

        public const string BrightBlack = $"{Esc}100m";
        public const string BrightRed = $"{Esc}101m";
        public const string BrightGreen = $"{Esc}102m";
        public const string BrightYellow = $"{Esc}103m";
        public const string BrightBlue = $"{Esc}104m";
        public const string BrightMagenta = $"{Esc}105m";
        public const string BrightCyan = $"{Esc}106m";
        public const string BrightWhite = $"{Esc}107m";

        public static string Id(byte id)
        {
            return $"{Esc}48;5;{id}m";
        }

        public static string Rgb(byte r, byte g, byte b)
        {
            return $"{Esc}48;2;{r};{g};{b}m";
        }
    }
}
