namespace FinanceTracker.EFCore.Menu;

/// <summary>
/// Helper methods for console menu interactions.
/// </summary>
public static class MenuHelper
{
    public static int ShowMenu(string title, string[] options)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
        Console.WriteLine();

        for (int i = 0; i < options.Length; i++)
        {
            Console.WriteLine($"  {i + 1}. {options[i]}");
        }

        Console.WriteLine();
        Console.Write("Enter your choice: ");

        if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= options.Length)
        {
            return choice;
        }

        return -1;
    }

    public static string PromptString(string prompt, bool required = true)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            var input = Console.ReadLine()?.Trim() ?? "";

            if (!required || !string.IsNullOrEmpty(input))
            {
                return input;
            }

            Console.WriteLine("This field is required. Please try again.");
        }
    }

    public static int PromptInt(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            if (int.TryParse(Console.ReadLine(), out int value))
            {
                return value;
            }
            Console.WriteLine("Invalid number. Please try again.");
        }
    }

    public static decimal PromptDecimal(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal value))
            {
                return value;
            }
            Console.WriteLine("Invalid number. Please try again.");
        }
    }

    public static DateTime PromptDate(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (yyyy-MM-dd): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime value))
            {
                return value;
            }
            Console.WriteLine("Invalid date. Please use format yyyy-MM-dd.");
        }
    }

    public static void ShowSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[SUCCESS] {message}");
        Console.ResetColor();
    }

    public static void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    public static void ShowInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WaitForKey()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
    }
}
