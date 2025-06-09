using System.Diagnostics;
using Microsoft.Data.SqlClient;
using StackExchange.Redis;

namespace ShopCart.Api.Infrastructure;

public static class DockerContainerManager
{
    public static async Task EnsureContainersRunningAsync(WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment())
        {
            // Skip in non-development environments
            return;
        }

        try
        {
            // Check if Docker is available
            if (!await IsDockerRunningAsync())
            {
                Console.WriteLine("Docker is not running or not installed. Skipping container checks.");
                return;
            }

            // Check if containers are running
            var sqlServerRunning = await IsContainerRunningAsync("sqlserver");
            var redisRunning = await IsContainerRunningAsync("redis");

            // If any container is not running, start all containers using docker-compose
            if (!sqlServerRunning || !redisRunning)
            {
                Console.WriteLine("Some containers are not running. Starting all containers with docker-compose...");
                
                // Try to start containers with a timeout
                var startTask = StartDockerComposeAsync();
                if (await Task.WhenAny(startTask, Task.Delay(TimeSpan.FromMinutes(2))) != startTask)
                {
                    Console.WriteLine("WARNING: Docker Compose operation timed out after 2 minutes.");
                    Console.WriteLine("The application will continue, but services might not be available.");
                    return;
                }
                
                await startTask; // Get any exceptions
                
                // Wait for services to be ready
                await WaitForSqlServerReadyAsync(builder.Configuration);
                await WaitForRedisReadyAsync(builder.Configuration);
            }
            else
            {
                Console.WriteLine("All required containers are already running.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking Docker containers: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private static async Task<bool> IsDockerRunningAsync()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "info",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking if Docker is running: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> IsContainerRunningAsync(string containerName)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"ps --filter name={containerName} --format \"{{{{.Names}}}}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Warning when checking container {containerName}: {error}");
            }

            return !string.IsNullOrWhiteSpace(output);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking if container {containerName} is running: {ex.Message}");
            return false;
        }
    }

    private static async Task StartDockerComposeAsync()
    {
        Console.WriteLine("Starting containers with docker-compose...");
        
        // Find the solution directory (where compose.yaml is located)
        var currentDirectory = Directory.GetCurrentDirectory();
        var solutionDirectory = FindSolutionDirectory(currentDirectory);
        
        Console.WriteLine($"Using directory for docker-compose: {solutionDirectory}");
        
        // Check if compose file exists
        var composeFilePath = Path.Combine(solutionDirectory, "compose.yaml");
        var dockerComposeFilePath = Path.Combine(solutionDirectory, "docker-compose.yml");
        
        if (File.Exists(composeFilePath))
        {
            Console.WriteLine($"Found compose file: {composeFilePath}");
        }
        else if (File.Exists(dockerComposeFilePath))
        {
            Console.WriteLine($"Found docker-compose file: {dockerComposeFilePath}");
        }
        else
        {
            Console.WriteLine("WARNING: No compose.yaml or docker-compose.yml file found!");
            Console.WriteLine($"Directory contents: {string.Join(", ", Directory.GetFiles(solutionDirectory))}");
        }
        
        // Try using docker compose directly
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "compose up -d sqlserver redis",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = solutionDirectory
                }
            };

            Console.WriteLine("Executing: docker compose up -d sqlserver redis");
            process.Start();
            
            // Read output and error streams asynchronously
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            
            // Wait for process to exit with a timeout
            if (!process.WaitForExit(60000)) // 60 seconds timeout
            {
                Console.WriteLine("WARNING: Docker compose command is taking too long, but we'll continue waiting...");
            }
            
            var output = await outputTask;
            var error = await errorTask;

            Console.WriteLine("Docker compose command completed.");
            Console.WriteLine($"Output: {output}");
            
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Error: {error}");
            }

            if (process.ExitCode != 0)
            {
                Console.WriteLine($"Docker compose command failed with exit code: {process.ExitCode}");
                
                // Try alternative approach with docker-compose (hyphenated version)
                Console.WriteLine("Trying alternative command: docker-compose up -d sqlserver redis");
                await RunDockerComposeAlternativeAsync(solutionDirectory);
            }
            else
            {
                Console.WriteLine("Docker Compose started successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception when running docker compose: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Try alternative approach
            Console.WriteLine("Trying alternative command due to exception...");
            await RunDockerComposeAlternativeAsync(solutionDirectory);
        }
    }
    
    private static async Task RunDockerComposeAlternativeAsync(string workingDirectory)
    {
        try
        {
            // Try the hyphenated version (docker-compose)
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker-compose",
                    Arguments = "up -d sqlserver redis",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            if (!process.WaitForExit(60000)) // 60 seconds timeout
            {
                Console.WriteLine("WARNING: docker-compose command is taking too long, but we'll continue waiting...");
            }

            Console.WriteLine($"Alternative command output: {output}");
            
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Alternative command error: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception when running alternative docker-compose command: {ex.Message}");
        }
    }

    private static string FindSolutionDirectory(string startDirectory)
    {
        // Start from the current directory and go up until we find compose.yaml
        var directory = new DirectoryInfo(startDirectory);
        
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "compose.yaml")) || 
                File.Exists(Path.Combine(directory.FullName, "docker-compose.yml")) ||
                File.Exists(Path.Combine(directory.FullName, "docker-compose.yaml")))
            {
                return directory.FullName;
            }
            
            directory = directory.Parent;
        }
        
        // If we can't find it, return the current directory
        Console.WriteLine("Warning: Could not find compose.yaml in any parent directory. Using current directory.");
        return startDirectory;
    }

    private static async Task WaitForSqlServerReadyAsync(IConfiguration configuration)
    {
        Console.WriteLine("Waiting for SQL Server to be ready...");
        
        int retries = 0;
        const int maxRetries = 30;
        
        while (retries < maxRetries)
        {
            try
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                Console.WriteLine($"Trying to connect to SQL Server with connection string: {connectionString}");
                
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                Console.WriteLine("SQL Server is ready.");
                return;
            }
            catch (Exception ex)
            {
                retries++;
                Console.WriteLine($"SQL Server not ready (attempt {retries}/{maxRetries}): {ex.Message}");
                await Task.Delay(2000); // Wait 2 seconds before retrying
            }
        }
        
        Console.WriteLine("SQL Server did not become ready in the expected time.");
    }

    private static async Task WaitForRedisReadyAsync(IConfiguration configuration)
    {
        Console.WriteLine("Waiting for Redis to be ready...");
        
        int retries = 0;
        const int maxRetries = 15;
        
        while (retries < maxRetries)
        {
            try
            {
                var redisConnection = configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
                Console.WriteLine($"Trying to connect to Redis with connection string: {redisConnection}");
                
                using var redis = ConnectionMultiplexer.Connect(redisConnection);
                var db = redis.GetDatabase();
                await db.PingAsync();
                Console.WriteLine("Redis is ready.");
                return;
            }
            catch (Exception ex)
            {
                retries++;
                Console.WriteLine($"Redis not ready (attempt {retries}/{maxRetries}): {ex.Message}");
                await Task.Delay(1000); // Wait 1 second before retrying
            }
        }
        
        Console.WriteLine("Redis did not become ready in the expected time.");
    }
}