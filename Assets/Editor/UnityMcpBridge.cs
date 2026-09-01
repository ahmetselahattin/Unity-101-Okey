#if UNITY_EDITOR
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class UnityMcpBridge
{
    private static TcpListener _listener;
    private static Thread _listenerThread;
    private const int Port = 6400;

    static UnityMcpBridge()
    {
        EditorApplication.quitting += StopServer;
        StartServer();
    }

    private static void StartServer()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Loopback, Port);
            _listener.Start();
            _listenerThread = new Thread(ListenForClients) { IsBackground = true };
            _listenerThread.Start();
            Debug.Log($"[Unity MCP Bridge] Dinleme başladı: 127.0.0.1:{Port}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Unity MCP Bridge] Başlatma hatası: {ex.Message}");
        }
    }

    private static void StopServer()
    {
        _listener?.Stop();
        _listenerThread?.Abort();
    }

    private static void ListenForClients()
    {
        while (true)
        {
            try
            {
                var client = _listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
            catch { break; }
        }
    }

    private static void HandleClient(object obj)
    {
        using var client = (TcpClient)obj;
        using var stream = client.GetStream();
        var reader = new StreamReader(stream, Encoding.UTF8);
        var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        string request = reader.ReadLine();
        if (string.IsNullOrEmpty(request)) return;

        string response = "";
        EditorApplication.delayCall += () =>
        {
            response = ProcessCommand(request);
        };

        int timeout = 0;
        while (string.IsNullOrEmpty(response) && timeout < 50)
        {
            Thread.Sleep(50);
            timeout++;
        }

        writer.WriteLine(response);
    }

    private static string ProcessCommand(string cmd)
    {
        if (cmd == "get_hierarchy")
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            var sb = new StringBuilder();
            foreach (var go in roots) sb.Append(go.name).Append(", ");
            return sb.ToString().TrimEnd(',', ' ');
        }
        if (cmd == "play")
        {
            EditorApplication.isPlaying = true;
            return "Play moduna geçildi";
        }
        if (cmd == "stop")
        {
            EditorApplication.isPlaying = false;
            return "Durduruldu";
        }
        if (cmd == "refresh")
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            return "Refreshed";
        }
        return "Bilinmeyen komut";
    }
}
#endif
