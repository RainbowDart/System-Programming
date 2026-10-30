using TaskCoordinator;

var pass = 0;
var fail = 0;
void Check(bool ok, string n)
{
    if (ok)
    {
        pass++;
        Console.WriteLine("PASS " + n);
    }
    else
    {
        fail++;
        Console.WriteLine("FAIL " + n);
    }
}

var s = new DiagnosticService();

var one = await s.RunAsync("Сеть", TimeSpan.FromMilliseconds(20));
Check(one.Status == DiagnosticStatus.Completed, "одна диагностика завершается");

var all = await s.RunAllAsync();
Check(
    all.Count == 3 && all.All(x => x.Status == DiagnosticStatus.Completed),
    "WhenAll ждёт три диагностики"
);

var first = await s.FirstFinishedAsync();
Check(first == "Быстрая", "WhenAny возвращает первую завершившуюся");

// TODO: Добавить тесты кооперативной отмены, тайм-аута, неверного имени и отрицательного времени.

//тест на имя
try
{
    await s.RunAsync("", TimeSpan.FromMilliseconds(10));
    Check(false, "пустое имя кидает исключение");
}
catch (ArgumentException)
{
    Check(true, "пустое имя кидает исключение");
}

//тест на время
try
{
    await s.RunAsync("Тест", TimeSpan.FromMilliseconds(-10));
    Check(false, "отрицательное время кидает исключение");
}
catch (ArgumentOutOfRangeException)
{
    Check(true, "отрицательное время кидает исключение");
}

//тест на кооперативную отмену
using (var cts = new CancellationTokenSource())
{
    var task = s.RunAsync("Отмена", TimeSpan.FromSeconds(2), cts.Token);
    cts.Cancel();
    var res = await task;
    Check(res.Status == DiagnosticStatus.Cancelled, "кооперативная отмена возвращает статус Cancelled");
}

//тест на таймаут
try
{
    await s.RunWithTimeoutAsync("Тайм-аут", TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(50));
    Check(false, "тайм-аут вызывает TimeoutException");
}
catch (TimeoutException)
{
    Check(true, "тайм-аут вызывает TimeoutException");
}

Console.WriteLine($"Итого: {pass} пройдено, {fail} ошибок.");
return fail == 0 ? 0 : 1;