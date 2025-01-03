using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string url = "https://example.com/largefile.zip"; // Ersetze mit deiner URL
        string savePath = "largefile.zip"; // Zielpfad für die Datei

        try
        {
            long fileSize = await GetFileSizeAsync(url);
            Console.WriteLine($"Dateigröße: {fileSize} Bytes");

            if (fileSize > 0)
            {
                await DownloadFileAsync(url, savePath);
                Console.WriteLine("Download abgeschlossen.");
            }
            else
            {
                Console.WriteLine("Die Datei ist leer oder konnte nicht gefunden werden.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
        }
    }

    // Methode zum Abrufen der Dateigröße
    static async Task<long> GetFileSizeAsync(string url)
    {
        using (var client = new HttpClient())
        {
            try
            {
                // HTTP-Head-Request senden, um die Dateigröße zu ermitteln
                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                response.EnsureSuccessStatusCode();

                // Hole die "Content-Length" Header-Information
                if (response.Content.Headers.ContentLength.HasValue)
                {
                    return response.Content.Headers.ContentLength.Value;
                }
                else
                {
                    throw new Exception("Die Dateigröße konnte nicht ermittelt werden.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen der Dateigröße: " + ex.Message);
            }
        }
    }

    // Methode zum Herunterladen der Datei
    static async Task DownloadFileAsync(string url, string savePath)
    {
        using (var client = new HttpClient())
        {
            try
            {
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Herunterladen der Datei: " + ex.Message);
            }
        }
    }
}
