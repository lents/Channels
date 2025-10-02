// Пример BlockingCollection<T>
using System.Collections.Concurrent;

public class TraditionalProducerConsumer
{
    private static BlockingCollection<int> _queue = new BlockingCollection<int>();

    public static void Main()
    {
        // Производитель
        Task.Run(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                _queue.Add(i);
                Console.WriteLine($"Производитель: добавил {i}");
                Thread.Sleep(100); // Имитация работы
            }
            _queue.CompleteAdding(); // Сообщаем, что больше элементов не будет
        });

        // Потребитель
        Task.Run(() =>
        {
            try
            {
                foreach (var item in _queue.GetConsumingEnumerable()) // Блокирует, пока нет элементов
                {
                    Console.WriteLine($"Потребитель: получил {item}");
                    Thread.Sleep(200); // Имитация работы
                }
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Потребитель: очередь завершена.");
            }
        });

        Console.ReadLine(); // Ждем завершения
    }
}