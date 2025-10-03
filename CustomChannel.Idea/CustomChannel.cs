using System.Collections.Concurrent;

namespace CustomChannel.Idea
{
    public class CustomChannel<T>
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        // Этот семафор сигнализирует потребителям о наличии элементов.
        // initialCount: 0, потому что изначально очередь пуста.
        private readonly SemaphoreSlim _readSemaphore;

        // Этот семафор сигнализирует производителям о наличии свободного места (для BoundedChannel).
        // Если maxCapacity не указана, он не используется (или всегда разрешает доступ).
        private readonly SemaphoreSlim _writeSemaphore;

        private readonly int? _maxCapacity;
        private bool _isCompleted = false;
        private CancellationTokenSource _completionCts = new CancellationTokenSource();

        public CustomChannel() : this(null) { } // Неограниченный канал

        public CustomChannel(int? maxCapacity)
        {
            if (maxCapacity.HasValue && maxCapacity.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCapacity), "Capacity must be positive.");
            }

            _maxCapacity = maxCapacity;
            _readSemaphore = new SemaphoreSlim(0); // Изначально нет элементов для чтения

            if (_maxCapacity.HasValue)
            {
                // Если есть maxCapacity, этот семафор отслеживает свободное место.
                // initialCount: maxCapacity, потому что изначально все места свободны.
                _writeSemaphore = new SemaphoreSlim(maxCapacity.Value, maxCapacity.Value);
            }
            else
            {
                // Для неограниченного канала, writeSemaphore всегда разрешает доступ.
                // Мы могли бы использовать null и проверять, но для простоты создадим "фиктивный" семафор.
                _writeSemaphore = new SemaphoreSlim(int.MaxValue);
            }
        }

        // Позволяет производителю записать элемент в канал.
        public async ValueTask WriteAsync(T item, CancellationToken cancellationToken = default)
        {
            if (_isCompleted)
            {
                throw new InvalidOperationException("Channel is completed and cannot accept new writes.");
            }

            // Если канал ограничен, ждем свободного места
            if (_maxCapacity.HasValue)
            {
                await _writeSemaphore.WaitAsync(cancellationToken);
            }

            // Добавляем элемент в очередь. ConcurrentQueue потокобезопасна.
            _queue.Enqueue(item);

            // Сигнализируем одному ожидающему потребителю, что элемент доступен.
            // Release() увеличивает счетчик _readSemaphore, позволяя одному WaitAsync() завершиться.
            _readSemaphore.Release();
        }

        // Позволяет потребителю прочитать элемент из канала.
        public async ValueTask<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            // Объединяем CancellationToken из аргументов с внутренним completionCts,
            // чтобы чтение могло быть отменено либо извне, либо при завершении канала.
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _completionCts.Token);

            while (true) // Цикл для повторной попытки чтения, если канал завершается
            {
                await _readSemaphore.WaitAsync(linkedCts.Token); // Ждем наличия элемента

                // Если канал завершен И очередь пуста, бросаем исключение,
                // чтобы сигнализировать потребителю о конце данных.
                if (_isCompleted && _queue.IsEmpty)
                {
                    throw new InvalidOperationException("Channel is completed and no more items are available.");
                }

                // Пытаемся извлечь элемент из очереди. ConcurrentQueue потокобезопасна.
                if (_queue.TryDequeue(out T item))
                {
                    // Если канал ограничен, сигнализируем производителю, что место освободилось.
                    if (_maxCapacity.HasValue)
                    {
                        _writeSemaphore.Release();
                    }
                    return item;
                }
                else
                {
                    // Это может произойти, если несколько потребителей пробудились, но только один успел взять элемент.
                    // В этом случае, мы снова ждем. _readSemaphore.WaitAsync() уже успешно сработал,
                    // поэтому нам нужно снова увеличить его, чтобы не "съесть" разрешение,
                    // которое никто не использовал.
                    _readSemaphore.Release(); // Возвращаем разрешение
                                              // И пробуем снова ждать.
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
            Console.WriteLine("Channel has been completed.");
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