using TaskCoordinator;

var service = new DiagnosticService();
while (true)
{
    Console.WriteLine("\n1 — три диагностики; 2 — отмена; 3 — тайм-аут; 4 — WhenAny; 5 — CPU-задача; 0 — выход");
    var cmd = Console.ReadLine();
    if (cmd == "0")
        break;
    try
    {
        if (cmd == "1")
        {
            foreach (var x in await service.RunAllAsync())
                Console.WriteLine($"{x.Name}: {x.Status} {x.Details} ({x.Duration.TotalMilliseconds} мс)");
        }
        else if (cmd == "2")
        {
            using var cts = new CancellationTokenSource();
            var task = service.RunAsync("Долгая", TimeSpan.FromSeconds(5), cts.Token);
            await Task.Delay(400);
            cts.Cancel();
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Отменено пользователем.");
            }
        }
        else if (cmd == "3")
        {
            try
            {
                var res = await service.RunWithTimeoutAsync(
                    "Тайм-аут",
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(1)
                );
                Console.WriteLine($"{res.Name}: {res.Status}");
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        else if (cmd == "4")
            Console.WriteLine("Первой завершилась: " + await service.FirstFinishedAsync());
        else if (cmd == "5")
        {
            Console.WriteLine("Запуск CPU-задачи...");
            var result = await service.RunCpuBoundAsync("Процессор", 10_000_000);
            Console.WriteLine($"{result.Name}: {result.Status} ({result.Details}) за {result.Duration.TotalMilliseconds} мс");
        }
        else
            Console.WriteLine("Неизвестная команда.");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Отменено.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка {ex.GetType().Name}: {ex.Message}");
    }
}