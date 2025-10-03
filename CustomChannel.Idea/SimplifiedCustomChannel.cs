namespace CustomChannel.Idea
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public class SimplifiedCustomChannel<T>
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        // Этот семафор сигнализирует потребителям о наличии элементов.
        // initialCount: 0, потому что изначально очередь пуста.
        private readonly SemaphoreSlim _readSemaphore = new SemaphoreSlim(0);

        private bool _isCompleted = false;
        private CancellationTokenSource _completionCts = new CancellationTokenSource();

        public SimplifiedCustomChannel() { }

        // Позволяет производителю записать элемент в канал.
        public async ValueTask WriteAsync(T item, CancellationToken cancellationToken = default)
        {
            // Проверяем, не завершен ли канал. Если завершен, не позволяем записывать.
            if (_isCompleted)
            {
                throw new InvalidOperationException("Channel is completed and cannot accept new writes.");
            }

            // Добавляем элемент в очередь. ConcurrentQueue потокобезопасна и неблокирующая.
            _queue.Enqueue(item);

            // Сигнализируем одному ожидающему потребителю, что элемент доступен.
            // Release() увеличивает счетчик _readSemaphore, позволяя одному WaitAsync() завершиться.
            _readSemaphore.Release();
           
        }

        // Позволяет потребителю прочитать элемент из канала.
        public async ValueTask<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _completionCts.Token);

            while (true)
            {
                // Ждем наличия элемента. Если нет, блокируемся (асинхронно).
                await _readSemaphore.WaitAsync(linkedCts.Token);

                // Если канал завершен И очередь пуста, бросаем исключение,
                // чтобы сигнализировать потребителю о конце данных.
                if (_isCompleted && _queue.IsEmpty)
                {
                    throw new InvalidOperationException("Channel is completed and no more items are available.");
                }

                // Пытаемся извлечь элемент из очереди. ConcurrentQueue потокобезопасна.
                if (_queue.TryDequeue(out T item))
                {
                    return item;
                }
                else
                {
                    // Это может произойти, если несколько потребителей пробудились, но только один успел взять элемент.
                    // В этом случае, мы снова ждем. _readSemaphore.WaitAsync() уже успешно сработал,
                    // поэтому нам нужно снова увеличить его, чтобы не "съесть" разрешение,
                    // которое никто не использовал, и другой потребитель мог его получить.
                    _readSemaphore.Release(); // Возвращаем разрешение обратно
                                              // И пробуем снова ждать в следующем цикле.
                }
            }
        }

        // Сигнализирует, что производители закончили запись.
        // После вызова, ReadAsync будет возвращать исключение, когда очередь опустеет.
        public void Complete()
        {
            _isCompleted = true;
            // Отменяем все ожидающие чтения, чтобы они могли проверить _isCompleted
            _completionCts.Cancel();
            Console.WriteLine("Simplified Channel has been completed.");
        }

        // Асинхронный итератор для удобного чтения всех элементов
        public async IAsyncEnumerable<T> ReadAllAsync()
        {
            while (!_isCompleted || !_queue.IsEmpty)
            {
                yield return await ReadAsync();
            }
        }
    }
}