using System.Globalization;
using library_pr1;

namespace BuildAnalyzer.App;

/// <summary>
/// Точка входа консольного приложения. Отвечает за весь ввод и вывод данных.
/// Все расчёты делегируются библиотеке <see cref="BuildAnalyzer.Core"/>,
/// которая ничего не знает о консоли.
/// </summary>
internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("=== Анализатор игровой сборки персонажа ===");
        Console.WriteLine();

        string name = ReadNonEmptyString("Имя персонажа: ");
        double baseAttack = ReadDouble("Базовая атака: ");
        double weaponAttack = ReadDouble("Атака оружия: ");
        double criticalChance = ReadPercentage("Шанс критического удара (%): ");
        double criticalDamagePercent = ReadPercentage("Критический урон (%): ");

        var character = new Character(name, baseAttack, weaponAttack, criticalChance, criticalDamagePercent);

        BuildAnalysisResult result = CharacterBuildAnalyzer.Analyze(character);

        PrintResult(character.Name, result);
    }

    /// <summary>
    /// Выводит результат анализа в консоль в читаемом виде.
    /// </summary>
    /// <param name="characterName">Имя проанализированного персонажа.</param>
    /// <param name="result">Рассчитанный результат анализа.</param>
    private static void PrintResult(string characterName, BuildAnalysisResult result)
    {
        Console.WriteLine();
        Console.WriteLine($"--- Результат для персонажа \"{characterName}\" ---");
        Console.WriteLine($"Итоговая атака:        {result.FinalAttack:F2}");
        Console.WriteLine($"Шанс критического удара: {result.CriticalChancePercent:F2}%");
        Console.WriteLine($"Критический урон:      {result.CriticalDamage:F2}");
        Console.WriteLine($"Средний урон:          {result.AverageDamage:F2}");
        Console.WriteLine($"Рейтинг сборки:        {TranslateRating(result.Rating)}");
    }

    /// <summary>
    /// Преобразует значение <see cref="BuildRating"/> в его название на русском языке.
    /// </summary>
    /// <param name="rating">Рейтинг для преобразования.</param>
    /// <returns>Текст рейтинга на русском языке.</returns>
    private static string TranslateRating(BuildRating rating) => rating switch
    {
        BuildRating.Weak => "Слабая",
        BuildRating.Normal => "Нормальная",
        BuildRating.Good => "Хорошая",
        BuildRating.Excellent => "Отличная",
        _ => rating.ToString()
    };

    /// <summary>
    /// Запрашивает у пользователя ввод, пока не будет введена непустая строка.
    /// </summary>
    /// <param name="prompt">Текст, показываемый пользователю.</param>
    /// <returns>Введённая непустая строка.</returns>
    private static string ReadNonEmptyString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            Console.WriteLine("Значение не может быть пустым. Попробуйте снова.");
        }
    }

    /// <summary>
    /// Запрашивает у пользователя ввод, пока не будет введено корректное
    /// неотрицательное число. Принимает и точку, и запятую как разделитель дробной части.
    /// </summary>
    /// <param name="prompt">Текст, показываемый пользователю.</param>
    /// <returns>Введённое неотрицательное число.</returns>
    private static double ReadDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (TryParseInvariant(input, out double value) && value >= 0)
            {
                return value;
            }

            Console.WriteLine("Введите корректное неотрицательное число. Попробуйте снова.");
        }
    }

    /// <summary>
    /// Запрашивает у пользователя ввод, пока не будет введено корректное
    /// значение процента (от 0 до 100).
    /// </summary>
    /// <param name="prompt">Текст, показываемый пользователю.</param>
    /// <returns>Введённое значение процента.</returns>
    private static double ReadPercentage(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (TryParseInvariant(input, out double value) && value >= 0 && value <= 100)
            {
                return value;
            }

            Console.WriteLine("Введите число от 0 до 100. Попробуйте снова.");
        }
    }

    /// <summary>
    /// Преобразует строку в число типа double, принимая и точку, и запятую
    /// как разделитель дробной части, независимо от текущей культуры системы.
    /// </summary>
    /// <param name="input">Исходная строка, введённая пользователем.</param>
    /// <param name="value">Результат разбора, если он прошёл успешно.</param>
    /// <returns><see langword="true"/>, если разбор прошёл успешно; иначе <see langword="false"/>.</returns>
    private static bool TryParseInvariant(string? input, out double value)
    {
        string normalized = (input ?? string.Empty).Trim().Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}