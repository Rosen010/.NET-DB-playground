using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.EFCore.Services;

namespace FinanceTracker.EFCore.Menu;

public class CategoryMenu
{
    private readonly CategoryService _categoryService;

    public CategoryMenu(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            var choice = MenuHelper.ShowMenu("Category Management", new[]
            {
                "List all categories",
                "List expense categories",
                "List income categories",
                "View category by ID",
                "Create new category",
                "Update category",
                "Delete category",
                "Back to main menu"
            });

            switch (choice)
            {
                case 1: await ListCategoriesAsync(null); break;
                case 2: await ListCategoriesAsync(CategoryType.Expense); break;
                case 3: await ListCategoriesAsync(CategoryType.Income); break;
                case 4: await ViewCategoryAsync(); break;
                case 5: await CreateCategoryAsync(); break;
                case 6: await UpdateCategoryAsync(); break;
                case 7: await DeleteCategoryAsync(); break;
                case 8: return;
                default: MenuHelper.ShowError("Invalid choice."); break;
            }
        }
    }

    private async Task ListCategoriesAsync(CategoryType? type)
    {
        var categories = type.HasValue
            ? await _categoryService.GetByTypeAsync(type.Value)
            : await _categoryService.GetAllAsync();

        Console.WriteLine();
        Console.WriteLine("ID    | Name                 | Type     | Icon            | Color");
        Console.WriteLine(new string('-', 75));

        foreach (var cat in categories)
        {
            Console.WriteLine($"{cat.Id,-5} | {cat.Name,-20} | {cat.Type,-8} | {cat.Icon ?? "-",-15} | {cat.Color ?? "-"}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task ViewCategoryAsync()
    {
        var id = MenuHelper.PromptInt("Enter category ID");
        var category = await _categoryService.GetByIdAsync(id);

        if (category == null)
            MenuHelper.ShowError($"Category with ID {id} not found.");
        else
        {
            Console.WriteLine();
            Console.WriteLine($"ID:    {category.Id}");
            Console.WriteLine($"Name:  {category.Name}");
            Console.WriteLine($"Type:  {category.Type}");
            Console.WriteLine($"Icon:  {category.Icon ?? "(none)"}");
            Console.WriteLine($"Color: {category.Color ?? "(none)"}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task CreateCategoryAsync()
    {
        Console.WriteLine();
        MenuHelper.ShowInfo("Creating new category...");

        var name = MenuHelper.PromptString("Enter category name");

        Console.WriteLine("Category type: 1. Expense  2. Income");
        var typeChoice = MenuHelper.PromptInt("Enter choice (1 or 2)");

        var category = new Category
        {
            Name = name,
            Type = typeChoice == 2 ? CategoryType.Income : CategoryType.Expense,
            Icon = MenuHelper.PromptString("Enter icon name (optional)", required: false),
            Color = MenuHelper.PromptString("Enter color hex (optional)", required: false)
        };

        if (string.IsNullOrEmpty(category.Icon)) category.Icon = null;
        if (string.IsNullOrEmpty(category.Color)) category.Color = null;

        try
        {
            var created = await _categoryService.CreateAsync(category);
            MenuHelper.ShowSuccess($"Category created with ID: {created.Id}");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to create category: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task UpdateCategoryAsync()
    {
        var id = MenuHelper.PromptInt("Enter category ID to update");
        var category = await _categoryService.GetByIdAsync(id);

        if (category == null)
        {
            MenuHelper.ShowError($"Category with ID {id} not found.");
            MenuHelper.WaitForKey();
            return;
        }

        Console.WriteLine($"Current name: {category.Name}");
        var newName = MenuHelper.PromptString("Enter new name (or Enter to keep)", required: false);
        if (!string.IsNullOrEmpty(newName)) category.Name = newName;

        Console.WriteLine($"Current icon: {category.Icon ?? "(none)"}");
        var newIcon = MenuHelper.PromptString("Enter new icon (or Enter to keep)", required: false);
        if (!string.IsNullOrEmpty(newIcon)) category.Icon = newIcon;

        Console.WriteLine($"Current color: {category.Color ?? "(none)"}");
        var newColor = MenuHelper.PromptString("Enter new color (or Enter to keep)", required: false);
        if (!string.IsNullOrEmpty(newColor)) category.Color = newColor;

        try
        {
            await _categoryService.UpdateAsync(category);
            MenuHelper.ShowSuccess("Category updated successfully.");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to update category: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task DeleteCategoryAsync()
    {
        var id = MenuHelper.PromptInt("Enter category ID to delete");

        Console.Write("Are you sure? (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y")
        {
            MenuHelper.ShowInfo("Deletion cancelled.");
            MenuHelper.WaitForKey();
            return;
        }

        try
        {
            var success = await _categoryService.DeleteAsync(id);
            if (success)
                MenuHelper.ShowSuccess("Category deleted successfully.");
            else
                MenuHelper.ShowError($"Category with ID {id} not found or has related records.");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to delete category: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }
}
