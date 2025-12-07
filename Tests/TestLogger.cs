using System.Text;

namespace Tests
{
    public static class TestLogger
    {
        private static readonly string LogFilePath = "Tests/Resources/test-execution-log.txt";
        private static readonly object LockObject = new object();

        public static void LogTestStart(string testName, string testType)
        {
            var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 🚀 STARTING {testType}: {testName}";
            LogMessage(message);
            Console.WriteLine(message);
        }

        public static void LogTestPass(string testName, string testType, TimeSpan duration)
        {
            var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ✅ PASSED {testType}: {testName} ({duration.TotalMilliseconds:F0}ms)";
            LogMessage(message);
            Console.WriteLine(message);
        }

        public static void LogTestFail(string testName, string testType, string error, TimeSpan duration)
        {
            var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ❌ FAILED {testType}: {testName} ({duration.TotalMilliseconds:F0}ms)\n   Error: {error}";
            LogMessage(message);
            Console.WriteLine(message);
        }

        public static void LogCredentialTest(string email, string password, bool success)
        {
            var status = success ? "✅ SUCCESS" : "❌ FAILED";
            var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 🔐 CREDENTIAL TEST: {email} with password '{password}' - {status}";
            LogMessage(message);
            Console.WriteLine(message);
        }

        public static void LogBugFound(string bugDescription, string priority)
        {
            var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 🐛 BUG FOUND ({priority}): {bugDescription}";
            LogMessage(message);
            Console.WriteLine(message);
        }

        public static void LogSummary(int totalTests, int passed, int failed, TimeSpan totalTime)
        {
            var successRate = totalTests > 0 ? (double)passed / totalTests * 100 : 0;
            var message = $@"
[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 📊 TEST EXECUTION SUMMARY
=================================================================
Total Tests: {totalTests}
✅ Passed: {passed}
❌ Failed: {failed}
📈 Success Rate: {successRate:F1}%
⏱️ Total Time: {totalTime:mm\\:ss\\.fff}
=================================================================";
            LogMessage(message);
            Console.WriteLine(message);
        }

        private static void LogMessage(string message)
        {
            lock (LockObject)
            {
                try
                {
                    var directory = Path.GetDirectoryName(LogFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.AppendAllText(LogFilePath, message + Environment.NewLine, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }
        }

        public static void ClearLog()
        {
            lock (LockObject)
            {
                try
                {
                    if (File.Exists(LogFilePath))
                    {
                        File.Delete(LogFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to clear log file: {ex.Message}");
                }
            }
        }
    }
}