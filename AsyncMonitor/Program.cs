using AsyncMonitor;

var service = new CheckService();
var checks = new (string Name, TimeSpan Delay)[]
{
    ("Сеть", TimeSpan.FromMilliseconds(700)),
    ("База данных", TimeSpan.FromMilliseconds(900)),
    ("Файлы", TimeSpan.FromMilliseconds(500))
};

while (true)
{
    Console.WriteLine("\n1 — одна проверка; 2 — последовательно; 3 — параллельно; 4 — отмена; 0 — выход");
    var cmd = Console.ReadLine();
    if (cmd == "0")
        break;

    try
    {
        if (cmd == "1")
        {
            // TODO 8: Вызвать метод с await, не применять .Wait() или .Result.
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.CheckServiceAsync("Одиночная проверка", TimeSpan.FromSeconds(1));
            sw.Stop();
            Console.WriteLine($"Одна проверка заняла: {sw.ElapsedMilliseconds} мс");
            Print(new[] { r });
        }
        else if (cmd == "2")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunSequentialAsync(checks);
            sw.Stop();
            Console.WriteLine($"Последовательно: {sw.ElapsedMilliseconds} мс");
            Print(r);
        }
        else if (cmd == "3")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunParallelAsync(checks);
            sw.Stop();
            Console.WriteLine($"Параллельно: {sw.ElapsedMilliseconds} мс");
            Print(r);
        }
        else if (cmd == "4")
        {
            // TODO 10: Добавить режим контроля/отмены длительной проверки и показать,
            // что меню не зависает на ожидании Task.Delay.
            Console.WriteLine("Запущена проверка на 10 секунд. Нажмите Enter для отмены...");

            using var cts = new CancellationTokenSource();

            //фоновая задача для чтения Enter
            _ = Task.Run(() =>
            {
                Console.ReadLine();
                cts.Cancel();
            });

            try
            {
                var r = await service.CheckServiceAsync("Долгая проверка", TimeSpan.FromSeconds(10), cts.Token);
                Print(new[] { r });
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Проверка была успешно отменена!");
            }
        }
        else
            Console.WriteLine("Неизвестная команда.");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Проверка отменена.");
    }
    catch (Exception ex)
    {
        // TODO 9: Не скрывать исключение; сообщить тип и причину.
        Console.WriteLine($"Ошибка [{ex.GetType().Name}]: {ex.Message}");
    }
}

static void Print(IEnumerable<CheckResult> xs)
{
    foreach (var x in xs)
    {
        Console.WriteLine($" - {x.Name}: {(x.Success ? "OK" : "FAIL")} за {x.Duration.TotalMilliseconds} мс ({x.Message})");
    }
}