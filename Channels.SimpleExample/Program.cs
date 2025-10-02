// Пример Channel<T> (общая идея)
using System.Threading.Channels;

public class AsyncProducerConsumer
{
    private static Channel<int> _channel = Channel.CreateUnbounded<int>(); // Создаем неограниченный канал

    public static void Main()
    {
        // Производитель
        _ = Task.Run(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                await _channel.Writer.WriteAsync(i); // Асинхронная запись
                Console.WriteLine($"Async Производитель: добавил {i}");
                await Task.Delay(100);
            }
            _channel.Writer.Complete(); // Сообщаем, что больше элементов не будет
        });

        // Потребитель
        _ = Task.Run(async () =>
        {
            await foreach (var item in _channel.Reader.ReadAllAsync()) // Асинхронное чтение
            {
                Console.WriteLine($"Async Потребитель: получил {item}");
                await Task.Delay(200);
            }
            Console.WriteLine("Async Потребитель: канал завершен.");
        });

        Console.WriteLine("Нажмите Enter для завершения...");
        Console.ReadLine();
    }
}