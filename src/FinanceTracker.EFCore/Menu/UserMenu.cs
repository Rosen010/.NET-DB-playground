using FinanceTracker.Domain.Entities;
using FinanceTracker.EFCore.Services;

namespace FinanceTracker.EFCore.Menu;

public class UserMenu
{
    private readonly UserService _userService;

    public UserMenu(UserService userService)
    {
        _userService = userService;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            var choice = MenuHelper.ShowMenu("User Management", new[]
            {
                "List all users",
                "View user by ID",
                "Create new user",
                "Update user",
                "Delete user",
                "Back to main menu"
            });

            switch (choice)
            {
                case 1: await ListUsersAsync(); break;
                case 2: await ViewUserAsync(); break;
                case 3: await CreateUserAsync(); break;
                case 4: await UpdateUserAsync(); break;
                case 5: await DeleteUserAsync(); break;
                case 6: return;
                default: MenuHelper.ShowError("Invalid choice."); break;
            }
        }
    }

    private async Task ListUsersAsync()
    {
        var users = await _userService.GetAllAsync();

        Console.WriteLine();
        Console.WriteLine("ID    | Name                 | Email                          | Created");
        Console.WriteLine(new string('-', 80));

        foreach (var user in users)
        {
            Console.WriteLine($"{user.Id,-5} | {user.Name,-20} | {user.Email,-30} | {user.CreatedAt:yyyy-MM-dd}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task ViewUserAsync()
    {
        var id = MenuHelper.PromptInt("Enter user ID");
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
            MenuHelper.ShowError($"User with ID {id} not found.");
        else
        {
            Console.WriteLine();
            Console.WriteLine($"ID:         {user.Id}");
            Console.WriteLine($"Name:       {user.Name}");
            Console.WriteLine($"Email:      {user.Email}");
            Console.WriteLine($"Created At: {user.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task CreateUserAsync()
    {
        Console.WriteLine();
        MenuHelper.ShowInfo("Creating new user...");

        var user = new User
        {
            Name = MenuHelper.PromptString("Enter name"),
            Email = MenuHelper.PromptString("Enter email")
        };

        try
        {
            var created = await _userService.CreateAsync(user);
            MenuHelper.ShowSuccess($"User created with ID: {created.Id}");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to create user: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task UpdateUserAsync()
    {
        var id = MenuHelper.PromptInt("Enter user ID to update");
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
        {
            MenuHelper.ShowError($"User with ID {id} not found.");
            MenuHelper.WaitForKey();
            return;
        }

        Console.WriteLine($"Current name: {user.Name}");
        var newName = MenuHelper.PromptString("Enter new name (or press Enter to keep current)", required: false);
        if (!string.IsNullOrEmpty(newName)) user.Name = newName;

        Console.WriteLine($"Current email: {user.Email}");
        var newEmail = MenuHelper.PromptString("Enter new email (or press Enter to keep current)", required: false);
        if (!string.IsNullOrEmpty(newEmail)) user.Email = newEmail;

        try
        {
            await _userService.UpdateAsync(user);
            MenuHelper.ShowSuccess("User updated successfully.");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to update user: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task DeleteUserAsync()
    {
        var id = MenuHelper.PromptInt("Enter user ID to delete");

        Console.Write("Are you sure? This will delete all related data (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y")
        {
            MenuHelper.ShowInfo("Deletion cancelled.");
            MenuHelper.WaitForKey();
            return;
        }

        try
        {
            var success = await _userService.DeleteAsync(id);
            if (success)
                MenuHelper.ShowSuccess("User deleted successfully.");
            else
                MenuHelper.ShowError($"User with ID {id} not found.");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to delete user: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }
}
