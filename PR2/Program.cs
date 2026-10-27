using System.Net.Http.Json;
using System.Text.Json;

var client = new HttpClient { BaseAddress = new Uri("https://jsonplaceholder.typicode.com/") };

while (true)
{
    Console.WriteLine("\n1. Показать задачи пользователя");
    Console.WriteLine("2. Найти задачу по ID");
    Console.WriteLine("3. Создать задачу");
    Console.WriteLine("4. Изменить статус задачи");
    Console.WriteLine("5. Удалить задачу");
    Console.WriteLine("0. Выход");
    Console.Write("\nВыберите действие: ");

    try
    {
        switch (Console.ReadLine())
        {
            case "1": await ShowUserTodos(); break;
            case "2": await FindTodo(); break;
            case "3": await CreateTodo(); break;
            case "4": await UpdateStatus(); break;
            case "5": await DeleteTodo(); break;
            case "0": return;
            default: Console.WriteLine("Неверный пункт меню."); break;
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Ошибка HTTP: {ex.Message} (код: {ex.StatusCode})");
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Ошибка разбора JSON: {ex.Message}");
    }
}

int ReadInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out int value)) return value;
        Console.WriteLine("Введите целое число.");
    }
}

void Print(Todo t) =>
    Console.WriteLine($"[{t.Id}] {t.Title} — {(t.Completed ? "выполнена" : "не выполнена")}");

async Task ShowUserTodos()
{
    int userId = ReadInt("ID пользователя: ");
    List<Todo>? todos = await client.GetFromJsonAsync<List<Todo>>($"todos?userId={userId}");

    if (todos == null || todos.Count == 0)
    {
        Console.WriteLine("Задач не найдено.");
        return;
    }
    todos.ForEach(Print);
}

async Task FindTodo()
{
    int id = ReadInt("ID задачи: ");
    var response = await client.GetAsync($"todos/{id}");

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Не удалось получить задачу: {(int)response.StatusCode} {response.ReasonPhrase}");
        return;
    }
    var todo = await response.Content.ReadFromJsonAsync<Todo>();
    if (todo != null) Print(todo);
}

async Task CreateTodo()
{
    int userId = ReadInt("ID пользователя: ");
    Console.Write("Название задачи: ");
    string title = Console.ReadLine() ?? "";

    var newTodo = new Todo { UserId = userId, Title = title, Completed = false };
    var response = await client.PostAsJsonAsync("todos", newTodo);

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Ошибка создания: {(int)response.StatusCode}");
        return;
    }
    var created = await response.Content.ReadFromJsonAsync<Todo>();
    Console.WriteLine("Задача создана:");
    if (created != null) Print(created);
}

async Task UpdateStatus()
{
    int id = ReadInt("ID задачи: ");
    Console.Write("Выполнена? (y/n): ");
    bool completed = Console.ReadLine()?.Trim().ToLower() == "y";

    var content = JsonContent.Create(new { completed });
    var response = await client.PatchAsync($"todos/{id}", content);

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Ошибка изменения: {(int)response.StatusCode}");
        return;
    }
    var updated = await response.Content.ReadFromJsonAsync<Todo>();
    Console.WriteLine("Задача обновлена:");
    if (updated != null) Print(updated);
}

async Task DeleteTodo()
{
    int id = ReadInt("ID задачи: ");
    var response = await client.DeleteAsync($"todos/{id}");

    Console.WriteLine(response.IsSuccessStatusCode
        ? "Задача удалена."
        : $"Ошибка удаления: {(int)response.StatusCode}");
}

public class Todo
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool Completed { get; set; }
}