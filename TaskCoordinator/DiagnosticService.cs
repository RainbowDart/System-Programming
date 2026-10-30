using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TaskCoordinator;

public sealed class DiagnosticService
{
    public async Task<DiagnosticResult> RunAsync(string name, TimeSpan duration, CancellationToken token = default)
    {
        // TODO 1: Проверить имя и неотрицательную длительность.
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя диагностики не может быть пустым.", nameof(name));
        if (duration < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Длительность не может быть отрицательной.");

        var sw = Stopwatch.StartNew();
        try
        {
            // TODO 2: Передать token в Task.Delay.
            await Task.Delay(duration, token);
            sw.Stop();

            // TODO 3: Вернуть статус Completed и длительность.
            return new DiagnosticResult(name, DiagnosticStatus.Completed, "Успешно", sw.Elapsed);
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            //возвращаем статус, если токен был отменен
            return new DiagnosticResult(name, DiagnosticStatus.Cancelled, "Отменено", sw.Elapsed);
        }
    }

    public async Task<DiagnosticResult> RunCpuBoundAsync(string name, int iterations, CancellationToken token = default)
    {
        var sw = Stopwatch.StartNew();

        // TODO 4: Перенести CPU-bound цикл в Task.Run; регулярно проверять token.
        return await Task.Run(() =>
        {
            try
            {
                long sum = 0;
                for (int i = 0; i < iterations; i++)
                {
                    if (i % 1000 == 0) token.ThrowIfCancellationRequested();
                    sum += i % 7;
                }
                sw.Stop();
                return new DiagnosticResult(name, DiagnosticStatus.Completed, $"Сумма: {sum}", sw.Elapsed);
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                return new DiagnosticResult(name, DiagnosticStatus.Cancelled, "Вычисления отменены", sw.Elapsed);
            }
        }, token);
    }

    public async Task<IReadOnlyList<DiagnosticResult>> RunAllAsync(CancellationToken token = default)
    {
        var tasks = new[]
        {
            RunAsync("Сеть", TimeSpan.FromMilliseconds(600), token),
            RunAsync("Диск", TimeSpan.FromMilliseconds(900), token),
            RunAsync("База", TimeSpan.FromMilliseconds(700), token)
        };

        // TODO 5: Дождаться всех задач через WhenAll.
        var results = await Task.WhenAll(tasks);
        return results;
    }

    public async Task<DiagnosticResult> RunWithTimeoutAsync(string name, TimeSpan duration, TimeSpan timeout, CancellationToken token = default)
    {
        // TODO 6: Создать linked CancellationTokenSource на основе внешнего token.
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);

        // TODO 7: Настроить CancelAfter(timeout).
        linkedCts.CancelAfter(timeout);

        try
        {
            return await RunAsync(name, duration, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            // TODO 8: Отличить тайм-аут от отмены вызывающей стороны.
            if (token.IsCancellationRequested)
            {
                throw; //отмена
            }

            //если токен не отменить то сработает таймаут
            throw new TimeoutException($"Диагностика '{name}' превысила лимит времени в {timeout.TotalSeconds} сек.");
        }
    }

    public async Task<string> FirstFinishedAsync(CancellationToken token = default)
    {
        var tasks = new[]
        {
            RunAsync("Быстрая", TimeSpan.FromMilliseconds(150), token),
            RunAsync("Средняя", TimeSpan.FromMilliseconds(350), token),
            RunAsync("Медленная", TimeSpan.FromMilliseconds(600), token)
        };

        // TODO 9: Использовать WhenAny, затем await первой задачи.
        var completedTask = await Task.WhenAny(tasks);
        var result = await completedTask;
        return result.Name;
    }

    // TODO 10: Добавить обёртку, которая возвращает статусы Failed/Cancelled
    // и текст ошибки вместо сокрытия исключений.
    public async Task<DiagnosticResult> RunSafeAsync(string name, TimeSpan duration, CancellationToken token = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return await RunAsync(name, duration, token);
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            return new DiagnosticResult(name, DiagnosticStatus.Cancelled, "Отменено пользователем", sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new DiagnosticResult(name, DiagnosticStatus.Failed, $"Ошибка: {ex.Message}", sw.Elapsed);
        }
    }
}