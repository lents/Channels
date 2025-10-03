namespace CustomChannel.Idea
{
    using System;
    using System.Threading.Tasks;

    public class SimplifiedChannelDemo
    {
        public SimplifiedChannelDemo()
        {
        }

        public static async Task Run()
        {
            Console.WriteLine("--- Simplified Custom Unbounded Channel Demo (1 Semaphore) ---");
            var channel = new SimplifiedCustomChannel<int>();

            int numProducers = 3;
            int numConsumers = 5;

            // Создаем и запускаем несколько производителей
            Task[] producerTasks = new Task[numProducers];
            for (int i = 0; i < numProducers; i++)
            {
                int producerId = i + 1;
                producerTasks[i] = Task.Run(async () =>
                {
                    for (int j = 0; j < 5; j++) // Каждый производитель запишет 5 элементов
                    {
                        int item = producerId * 100 + j;
                        Console.WriteLine($"[P{producerId}] Writing: {item}");
                        await channel.WriteAsync(item);
                        await Task.Delay(100); // Небольшая задержка
                    }
                });
            }

            // Создаем и запускаем несколько потребителей
            Task[] consumerTasks = new Task[numConsumers];
            for (int i = 0; i < numConsumers; i++)
            {
                int consumerId = i + 1;
                consumerTasks[i] = Task.Run(async () =>
                {
                    try
                    {
                        await foreach (var item in channel.ReadAllAsync())
                        {
                            Console.WriteLine($"[C{consumerId}] Read: {item}");
                            await Task.Delay(200); // Имитация обработки
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[C{consumerId}] Consumer error: {ex.Message}");
                    }
                    Console.WriteLine($"[C{consumerId}] Finished.");
                });
            }

            // Ждем завершения всех производителей
            await Task.WhenAll(producerTasks);
            Console.WriteLine("All producers finished writing.");

            // Сигнализируем каналу, что больше записей не будет
            channel.Complete();

            // Ждем завершения всех потребителей
            await Task.WhenAll(consumerTasks);
            Console.WriteLine("Simplified Channel demo finished.");
        }
    }
}