# Real-World Web API with Channels

This project demonstrates a real-world use case for .NET Channels in an ASP.NET Core Web API. It implements a background processing system where an API endpoint can quickly accept jobs and queue them for processing by a background service, without blocking the client.

## How it Works

1.  **Producer (`JobsController`)**: An API endpoint at `POST /api/jobs` accepts requests to create new jobs. It creates a `Job` object, writes it to a shared `Channel`, and immediately returns an `HTTP 202 Accepted` response to the client. This makes the API very responsive.
2.  **Channel (`ChannelService`)**: A singleton service that holds a `Channel<Job>`. This ensures all components in the application share the same queue.
3.  **Consumer (`BackgroundJobService`)**: A `BackgroundService` continuously reads jobs from the `Channel` in the background. When a job is received, it processes it (simulated by a `Task.Delay`).

This pattern is useful for offloading long-running tasks from the request thread, improving API responsiveness and reliability.

## How to Run

1.  Ensure you have the .NET 9 SDK (or newer) installed.
2.  Navigate to the `RealWorld.WebApi` directory in your terminal.
3.  Run the application using the following command:
    ```bash
    dotnet run
    ```
4.  The API will be available at `http://localhost:5000` and `https://localhost:5001`.

## How to Test

You can send a `POST` request to the `/api/jobs` endpoint to queue a new job. Use a tool like `curl` or Postman.

### Using cURL

Open a new terminal and run the following command:

```bash
curl -X POST http://localhost:5000/api/jobs \
-H "Content-Type: application/json" \
-d '{"TaskName": "Process video file"}'
```

You should receive a response similar to this, with a unique `jobId`:

```json
{
  "jobId": "a1b2c3d4-e5f6-..."
}
```

Check the console output of the running Web API. You will see logs indicating that the job has been queued and then, after a 5-second delay, processed by the background service.