using System.Threading.Channels;

public class UnboundedChannelExample
{
    public static async Task Main()
    {
        Console.WriteLine("--- Пример UnboundedChannel Producer/Consumer ---");

        // 1. Создаем неограниченный канал для целых чисел
        var channel = Channel.CreateUnbounded<int>();
        var writer = channel.Writer;
        var reader = channel.Reader;

        // 2. Задача Производителя
        var producerTask = Task.Run(async () =>
        {
            for (int i = 0; i < 15; i++)
            {
                await writer.WriteAsync(i); // Асинхронно записываем элемент
                Console.WriteLine($"[Производитель] Добавил: {i}");
                await Task.Delay(TimeSpan.FromMilliseconds(50)); // Производитель работает быстро
            }
            writer.Complete(); // Важно: сообщаем, что больше элементов не будет
            Console.WriteLine("[Производитель] Завершил работу.");
        });

        // 3. Задача Потребителя
        var consumerTask = Task.Run(async () =>
        {
            Console.WriteLine("[Потребитель] Начинает чтение...");
            await foreach (var item in reader.ReadAllAsync()) // Асинхронно читаем элементы
            {
                Console.WriteLine($"[Потребитель] Обработал: {item}");
                await Task.Delay(TimeSpan.FromMilliseconds(200)); // Потребитель работает медленнее
            }
            Console.WriteLine("[Потребитель] Завершил работу (канал пуст и завершен).");
        });

        // 4. Ждем завершения обеих задач
        await Task.WhenAll(producerTask, consumerTask);

        Console.WriteLine("--- Все задачи завершены ---");
    }
}