using FinanceTracker.AdoNet.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.AdoNet.Menu;

public class UserMenu
{
    private readonly UserRepository _repo;
    public UserMenu(UserRepository repo) => _repo = repo;

    public async Task ShowAsync()
    {
        while (true)
        {
            var choice = MenuHelper.ShowMenu("User Management (ADO.NET)", new[]
            { "List all", "View by ID", "Create", "Update", "Delete", "Back" });

            switch (choice)
            {
                case 1: await ListAsync(); break;
                case 2: await ViewAsync(); break;
                case 3: await CreateAsync(); break;
                case 4: await UpdateAsync(); break;
                case 5: await DeleteAsync(); break;
                case 6: return;
                default: MenuHelper.ShowError("Invalid choice."); break;
            }
        }
    }

    private async Task ListAsync()
    {
        var items = await _repo.GetAllAsync();
        Console.WriteLine("\nID    | Name                 | Email                          | Created");
        Console.WriteLine(new string('-', 80));
        foreach (var u in items)
            Console.WriteLine($"{u.Id,-5} | {u.Name,-20} | {u.Email,-30} | {u.CreatedAt:yyyy-MM-dd}");
        MenuHelper.WaitForKey();
    }

    private async Task ViewAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        var item = await _repo.GetByIdAsync(id);
        if (item == null) MenuHelper.ShowError("Not found.");
        else Console.WriteLine($"\nID: {item.Id}\nName: {item.Name}\nEmail: {item.Email}\nCreated: {item.CreatedAt}");
        MenuHelper.WaitForKey();
    }

    private async Task CreateAsync()
    {
        try
        {
            var user = new User { Name = MenuHelper.PromptString("Name"), Email = MenuHelper.PromptString("Email") };
            var id = await _repo.CreateAsync(user);
            MenuHelper.ShowSuccess($"Created with ID: {id}");
        }
        catch (Exception ex) { MenuHelper.ShowError(ex.Message); }
        MenuHelper.WaitForKey();
    }

    private async Task UpdateAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        var item = await _repo.GetByIdAsync(id);
        if (item == null) { MenuHelper.ShowError("Not found."); MenuHelper.WaitForKey(); return; }

        var name = MenuHelper.PromptString($"Name [{item.Name}]", false);
        if (!string.IsNullOrEmpty(name)) item.Name = name;
        var email = MenuHelper.PromptString($"Email [{item.Email}]", false);
        if (!string.IsNullOrEmpty(email)) item.Email = email;

        await _repo.UpdateAsync(item);
        MenuHelper.ShowSuccess("Updated.");
        MenuHelper.WaitForKey();
    }

    private async Task DeleteAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        Console.Write("Confirm delete? (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y") { MenuHelper.ShowInfo("Cancelled."); MenuHelper.WaitForKey(); return; }

        var success = await _repo.DeleteAsync(id);
        if (success) MenuHelper.ShowSuccess("Deleted.");
        else MenuHelper.ShowError("Not found.");
        MenuHelper.WaitForKey();
    }
}

public class CategoryMenu
{
    private readonly CategoryRepository _repo;
    public CategoryMenu(CategoryRepository repo) => _repo = repo;

    public async Task ShowAsync()
    {
        while (true)
        {
            var choice = MenuHelper.ShowMenu("Category Management (ADO.NET)", new[]
            { "List all", "List expenses", "List income", "View by ID", "Create", "Update", "Delete", "Back" });

            switch (choice)
            {
                case 1: await ListAsync(null); break;
                case 2: await ListAsync(CategoryType.Expense); break;
                case 3: await ListAsync(CategoryType.Income); break;
                case 4: await ViewAsync(); break;
                case 5: await CreateAsync(); break;
                case 6: await UpdateAsync(); break;
                case 7: await DeleteAsync(); break;
                case 8: return;
                default: MenuHelper.ShowError("Invalid choice."); break;
            }
        }
    }

    private async Task ListAsync(CategoryType? type)
    {
        var items = type.HasValue ? await _repo.GetByTypeAsync(type.Value) : await _repo.GetAllAsync();
        Console.WriteLine("\nID    | Name                 | Type     | Icon            | Color");
        Console.WriteLine(new string('-', 75));
        foreach (var c in items)
            Console.WriteLine($"{c.Id,-5} | {c.Name,-20} | {c.Type,-8} | {c.Icon ?? "-",-15} | {c.Color ?? "-"}");
        MenuHelper.WaitForKey();
    }

    private async Task ViewAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        var item = await _repo.GetByIdAsync(id);
        if (item == null) MenuHelper.ShowError("Not found.");
        else Console.WriteLine($"\nID: {item.Id}\nName: {item.Name}\nType: {item.Type}\nIcon: {item.Icon}\nColor: {item.Color}");
        MenuHelper.WaitForKey();
    }

    private async Task CreateAsync()
    {
        try
        {
            Console.WriteLine("Type: 1.Expense 2.Income");
            var cat = new Category
            {
                Name = MenuHelper.PromptString("Name"),
                Type = MenuHelper.PromptInt("Type (1/2)") == 2 ? CategoryType.Income : CategoryType.Expense,
                Icon = MenuHelper.PromptString("Icon (optional)", false),
                Color = MenuHelper.PromptString("Color (optional)", false)
            };
            if (string.IsNullOrEmpty(cat.Icon)) cat.Icon = null;
            if (string.IsNullOrEmpty(cat.Color)) cat.Color = null;
            var id = await _repo.CreateAsync(cat);
            MenuHelper.ShowSuccess($"Created with ID: {id}");
        }
        catch (Exception ex) { MenuHelper.ShowError(ex.Message); }
        MenuHelper.WaitForKey();
    }

    private async Task UpdateAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        var item = await _repo.GetByIdAsync(id);
        if (item == null) { MenuHelper.ShowError("Not found."); MenuHelper.WaitForKey(); return; }

        var name = MenuHelper.PromptString($"Name [{item.Name}]", false);
        if (!string.IsNullOrEmpty(name)) item.Name = name;

        await _repo.UpdateAsync(item);
        MenuHelper.ShowSuccess("Updated.");
        MenuHelper.WaitForKey();
    }

    private async Task DeleteAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        Console.Write("Confirm? (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y") { MenuHelper.ShowInfo("Cancelled."); MenuHelper.WaitForKey(); return; }

        try
        {
            var success = await _repo.DeleteAsync(id);
            if (success) MenuHelper.ShowSuccess("Deleted.");
            else MenuHelper.ShowError("Not found or has related records.");
        }
        catch (Exception ex) { MenuHelper.ShowError(ex.Message); }
        MenuHelper.WaitForKey();
    }
}

public class AccountMenu
{
    private readonly AccountRepository _repo;
    private readonly UserRepository _userRepo;
    public AccountMenu(AccountRepository repo, UserRepository userRepo) { _repo = repo; _userRepo = userRepo; }

    public async Task ShowAsync()
    {
        while (true)
        {
            var choice = MenuHelper.ShowMenu("Account Management (ADO.NET)", new[]
            { "List all", "List by user", "View by ID", "Create", "Update", "Delete", "Back" });

            switch (choice)
            {
                case 1: await ListAsync(); break;
                case 2: await ListByUserAsync(); break;
                case 3: await ViewAsync(); break;
                case 4: await CreateAsync(); break;
                case 5: await UpdateAsync(); break;
                case 6: await DeleteAsync(); break;
                case 7: return;
                default: MenuHelper.ShowError("Invalid choice."); break;
            }
        }
    }

    private async Task ListAsync()
    {
        var items = await _repo.GetAllAsync();
        Console.WriteLine("\nID    | User | Name                 | Type        | Balance       | Currency");
        Console.WriteLine(new string('-', 80));
        foreach (var a in items)
            Console.WriteLine($"{a.Id,-5} | {a.UserId,-4} | {a.Name,-20} | {a.Type,-11} | {a.Balance,13:N2} | {a.Currency}");
        MenuHelper.WaitForKey();
    }

    private async Task ListByUserAsync()
    {
        var userId = MenuHelper.PromptInt("Enter user ID");
        var items = await _repo.GetByUserIdAsync(userId);
        Console.WriteLine("\nID    | Name                 | Type        | Balance       | Currency");
        Console.WriteLine(new string('-', 75));
        foreach (var a in items)
            Console.WriteLine($"{a.Id,-5} | {a.Name,-20} | {a.Type,-11} | {a.Balance,13:N2} | {a.Currency}");
        MenuHelper.WaitForKey();
    }

    private async Task ViewAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        var item = await _repo.GetByIdAsync(id);
        if (item == null) MenuHelper.ShowError("Not found.");
        else Console.WriteLine($"\nID: {item.Id}\nUser: {item.UserId}\nName: {item.Name}\nType: {item.Type}\nBalance: {item.Balance:C}\nCurrency: {item.Currency}");
        MenuHelper.WaitForKey();
    }

    private async Task CreateAsync()
    {
        try
        {
            var users = await _userRepo.GetAllAsync();
            Console.WriteLine("Users: " + string.Join(", ", users.Select(u => $"{u.Id}:{u.Name}")));
            Console.WriteLine("Types: 1.Checking 2.Savings 3.CreditCard 4.Cash 5.Investment");

            var acc = new Account
            {
                UserId = MenuHelper.PromptInt("User ID"),
                Name = MenuHelper.PromptString("Name"),
                Type = (AccountType)MenuHelper.PromptInt("Type (1-5)"),
                Balance = MenuHelper.PromptDecimal("Balance"),
                Currency = MenuHelper.PromptString("Currency (USD)", false)
            };
            if (string.IsNullOrEmpty(acc.Currency)) acc.Currency = "USD";

            var id = await _repo.CreateAsync(acc);
            MenuHelper.ShowSuccess($"Created with ID: {id}");
        }
        catch (Exception ex) { MenuHelper.ShowError(ex.Message); }
        MenuHelper.WaitForKey();
    }

    private async Task UpdateAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        var item = await _repo.GetByIdAsync(id);
        if (item == null) { MenuHelper.ShowError("Not found."); MenuHelper.WaitForKey(); return; }

        var name = MenuHelper.PromptString($"Name [{item.Name}]", false);
        if (!string.IsNullOrEmpty(name)) item.Name = name;

        Console.Write($"Balance [{item.Balance}]: ");
        var balStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(balStr) && decimal.TryParse(balStr, out var bal)) item.Balance = bal;

        await _repo.UpdateAsync(item);
        MenuHelper.ShowSuccess("Updated.");
        MenuHelper.WaitForKey();
    }

    private async Task DeleteAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        Console.Write("Confirm? (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y") { MenuHelper.ShowInfo("Cancelled."); MenuHelper.WaitForKey(); return; }

        var success = await _repo.DeleteAsync(id);
        if (success) MenuHelper.ShowSuccess("Deleted.");
        else MenuHelper.ShowError("Not found.");
        MenuHelper.WaitForKey();
    }
}

public class TransactionMenu
{
    private readonly TransactionRepository _repo;
    private readonly AccountRepository _accRepo;
    private readonly CategoryRepository _catRepo;

    public TransactionMenu(TransactionRepository repo, AccountRepository accRepo, CategoryRepository catRepo)
    {
        _repo = repo; _accRepo = accRepo; _catRepo = catRepo;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            var choice = MenuHelper.ShowMenu("Transaction Management (ADO.NET)", new[]
            { "List recent", "List by account", "List by date range", "View by ID", "Create", "Update", "Delete", "Back" });

            switch (choice)
            {
                case 1: await ListRecentAsync(); break;
                case 2: await ListByAccountAsync(); break;
                case 3: await ListByDateRangeAsync(); break;
                case 4: await ViewAsync(); break;
                case 5: await CreateAsync(); break;
                case 6: await UpdateAsync(); break;
                case 7: await DeleteAsync(); break;
                case 8: return;
                default: MenuHelper.ShowError("Invalid choice."); break;
            }
        }
    }

    private async Task ListRecentAsync()
    {
        var items = await _repo.GetRecentAsync(20);
        DisplayTransactions(items);
        MenuHelper.WaitForKey();
    }

    private async Task ListByAccountAsync()
    {
        var accountId = MenuHelper.PromptInt("Account ID");
        var items = await _repo.GetByAccountIdAsync(accountId);
        DisplayTransactions(items);
        MenuHelper.WaitForKey();
    }

    private async Task ListByDateRangeAsync()
    {
        var start = MenuHelper.PromptDate("Start date");
        var end = MenuHelper.PromptDate("End date");
        var items = await _repo.GetByDateRangeAsync(start, end);
        DisplayTransactions(items);
        MenuHelper.WaitForKey();
    }

    private void DisplayTransactions(List<Transaction> items)
    {
        Console.WriteLine("\nID    | Date       | Cat | Amount       | Description");
        Console.WriteLine(new string('-', 70));
        foreach (var t in items)
        {
            var amt = t.Amount >= 0 ? $"+{t.Amount:N2}" : $"{t.Amount:N2}";
            Console.WriteLine($"{t.Id,-5} | {t.TransactionDate:yyyy-MM-dd} | {t.CategoryId,3} | {amt,12} | {t.Description ?? "-"}");
        }
    }

    private async Task ViewAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        var item = await _repo.GetByIdAsync(id);
        if (item == null) MenuHelper.ShowError("Not found.");
        else Console.WriteLine($"\nID: {item.Id}\nAccount: {item.AccountId}\nCategory: {item.CategoryId}\nAmount: {item.Amount:C}\nDate: {item.TransactionDate:yyyy-MM-dd}\nDescription: {item.Description}");
        MenuHelper.WaitForKey();
    }

    private async Task CreateAsync()
    {
        try
        {
            var accs = await _accRepo.GetAllAsync();
            Console.WriteLine("Accounts: " + string.Join(", ", accs.Select(a => $"{a.Id}:{a.Name}")));
            var cats = await _catRepo.GetAllAsync();
            Console.WriteLine("Categories: " + string.Join(", ", cats.Select(c => $"{c.Id}:{c.Name}")));

            var txn = new Transaction
            {
                AccountId = MenuHelper.PromptInt("Account ID"),
                CategoryId = MenuHelper.PromptInt("Category ID"),
                Amount = MenuHelper.PromptDecimal("Amount (+income/-expense)"),
                Description = MenuHelper.PromptString("Description (optional)", false),
                TransactionDate = MenuHelper.PromptDate("Date")
            };
            if (string.IsNullOrEmpty(txn.Description)) txn.Description = null;

            var id = await _repo.CreateAsync(txn);
            MenuHelper.ShowSuccess($"Created with ID: {id}");
        }
        catch (Exception ex) { MenuHelper.ShowError(ex.Message); }
        MenuHelper.WaitForKey();
    }

    private async Task UpdateAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        var item = await _repo.GetByIdAsync(id);
        if (item == null) { MenuHelper.ShowError("Not found."); MenuHelper.WaitForKey(); return; }

        Console.Write($"Amount [{item.Amount}]: ");
        var amtStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(amtStr) && decimal.TryParse(amtStr, out var amt)) item.Amount = amt;

        var desc = MenuHelper.PromptString($"Description [{item.Description}]", false);
        if (!string.IsNullOrEmpty(desc)) item.Description = desc;

        await _repo.UpdateAsync(item);
        MenuHelper.ShowSuccess("Updated.");
        MenuHelper.WaitForKey();
    }

    private async Task DeleteAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        Console.Write("Confirm? (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y") { MenuHelper.ShowInfo("Cancelled."); MenuHelper.WaitForKey(); return; }

        var success = await _repo.DeleteAsync(id);
        if (success) MenuHelper.ShowSuccess("Deleted.");
        else MenuHelper.ShowError("Not found.");
        MenuHelper.WaitForKey();
    }
}

public class BudgetMenu
{
    private readonly BudgetRepository _repo;
    private readonly UserRepository _userRepo;
    private readonly CategoryRepository _catRepo;

    public BudgetMenu(BudgetRepository repo, UserRepository userRepo, CategoryRepository catRepo)
    {
        _repo = repo; _userRepo = userRepo; _catRepo = catRepo;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            var choice = MenuHelper.ShowMenu("Budget Management (ADO.NET)", new[]
            { "List all", "List by user", "View by ID", "Create", "Update", "Delete", "Back" });

            switch (choice)
            {
                case 1: await ListAsync(); break;
                case 2: await ListByUserAsync(); break;
                case 3: await ViewAsync(); break;
                case 4: await CreateAsync(); break;
                case 5: await UpdateAsync(); break;
                case 6: await DeleteAsync(); break;
                case 7: return;
                default: MenuHelper.ShowError("Invalid choice."); break;
            }
        }
    }

    private async Task ListAsync()
    {
        var items = await _repo.GetAllAsync();
        Console.WriteLine("\nID    | User | Cat | Amount     | Period  | Start Date");
        Console.WriteLine(new string('-', 60));
        foreach (var b in items)
            Console.WriteLine($"{b.Id,-5} | {b.UserId,-4} | {b.CategoryId,3} | {b.Amount,10:N2} | {b.Period,-7} | {b.StartDate:yyyy-MM-dd}");
        MenuHelper.WaitForKey();
    }

    private async Task ListByUserAsync()
    {
        var userId = MenuHelper.PromptInt("User ID");
        var items = await _repo.GetByUserIdAsync(userId);
        Console.WriteLine("\nID    | Cat | Amount     | Period  | Start Date");
        Console.WriteLine(new string('-', 55));
        foreach (var b in items)
            Console.WriteLine($"{b.Id,-5} | {b.CategoryId,3} | {b.Amount,10:N2} | {b.Period,-7} | {b.StartDate:yyyy-MM-dd}");
        MenuHelper.WaitForKey();
    }

    private async Task ViewAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        var item = await _repo.GetByIdAsync(id);
        if (item == null) MenuHelper.ShowError("Not found.");
        else Console.WriteLine($"\nID: {item.Id}\nUser: {item.UserId}\nCategory: {item.CategoryId}\nAmount: {item.Amount:C}\nPeriod: {item.Period}\nStart: {item.StartDate:yyyy-MM-dd}");
        MenuHelper.WaitForKey();
    }

    private async Task CreateAsync()
    {
        try
        {
            var users = await _userRepo.GetAllAsync();
            Console.WriteLine("Users: " + string.Join(", ", users.Select(u => $"{u.Id}:{u.Name}")));
            var cats = await _catRepo.GetByTypeAsync(CategoryType.Expense);
            Console.WriteLine("Categories: " + string.Join(", ", cats.Select(c => $"{c.Id}:{c.Name}")));

            var budget = new Budget
            {
                UserId = MenuHelper.PromptInt("User ID"),
                CategoryId = MenuHelper.PromptInt("Category ID"),
                Amount = MenuHelper.PromptDecimal("Amount"),
                Period = MenuHelper.PromptInt("Period (1=Monthly, 2=Yearly)") == 2 ? BudgetPeriod.Yearly : BudgetPeriod.Monthly,
                StartDate = MenuHelper.PromptDate("Start date")
            };

            var id = await _repo.CreateAsync(budget);
            MenuHelper.ShowSuccess($"Created with ID: {id}");
        }
        catch (Exception ex) { MenuHelper.ShowError(ex.Message); }
        MenuHelper.WaitForKey();
    }

    private async Task UpdateAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        var item = await _repo.GetByIdAsync(id);
        if (item == null) { MenuHelper.ShowError("Not found."); MenuHelper.WaitForKey(); return; }

        Console.Write($"Amount [{item.Amount}]: ");
        var amtStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(amtStr) && decimal.TryParse(amtStr, out var amt)) item.Amount = amt;

        await _repo.UpdateAsync(item);
        MenuHelper.ShowSuccess("Updated.");
        MenuHelper.WaitForKey();
    }

    private async Task DeleteAsync()
    {
        var id = MenuHelper.PromptInt("Enter ID");
        Console.Write("Confirm? (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y") { MenuHelper.ShowInfo("Cancelled."); MenuHelper.WaitForKey(); return; }

        var success = await _repo.DeleteAsync(id);
        if (success) MenuHelper.ShowSuccess("Deleted.");
        else MenuHelper.ShowError("Not found.");
        MenuHelper.WaitForKey();
    }
}
