namespace sshmngr.Helper
{
    internal static class ConsoleHelper
    {
        internal static void ClearConsoleLine(int offset = 0)
        {
            Console.SetCursorPosition(0, Console.CursorTop + offset);
            Console.Write(new String(' ', Console.BufferWidth - 1));
            Console.SetCursorPosition(0, Console.CursorTop + offset + 1);
        }

        internal static void WriteToConsoleLine(int offset, string message)
        {
            var (currentLeft, currentTop) = Console.GetCursorPosition();

            Console.SetCursorPosition(0, Console.CursorTop + offset);
            Console.Write(message);
            
            Console.SetCursorPosition(currentLeft, currentTop);
        }
    }
}
