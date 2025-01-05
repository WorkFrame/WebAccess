namespace NetEti.FileTools
{
    /// <summary>
    /// Stellt Methoden zum Herunterladen von Dateien aus dem Internet bereit.
    /// </summary>
    /// <remarks>
    /// Autor: Erik Nagel
    ///
    /// 31.12.2024 Erik Nagel: erstellt.
    /// </remarks>
    public class WebOperator : IDisposable
    {
        #region IDisposable Member

        private bool _disposed; // = false wird vom System vorbelegt;

        /// <summary>
        /// Öffentliche Methode zum Aufräumen.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Hier wird aufgeräumt.
        /// </summary>
        /// <param name="disposing">False, wenn vom eigenen Destruktor auferufen.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this._disposed)
            {
                this._disposed = true;
            }
        }

        /// <summary>
        /// Finalizer: wird vom GarbageCollector aufgerufen.
        /// </summary>
        ~WebOperator()
        {
            this.Dispose(false);
        }

        #endregion IDisposable Member

        /// <summary>
        /// Gibt die Exception zurück, die beim letzten Task aufgetreten ist oder null.
        /// </summary>
        /// <returns></returns>
        public Exception? GetAndResetTaskException()
        {
            Exception? ex = this._taskException;
            this._taskException = null;
            return ex;
        }

        /// <summary>
        /// Lädt eine Datei aus dem Internet herunter.
        /// </summary>
        /// <param name="url">Die Webadresse.</param>
        /// <param name="savePath">Das Zielverzeichnis.</param>
        /// <param name="progress">Eine Variable mit einem Callback, über den der Prozessfortschritt zurückgegeben werden kann.</param>
        public void DownloadFile(string url, string savePath, IProgress<int>? progress = null)
        {
            DownloadFileAsync(url, savePath, progress).Wait();
        }

        /// <summary>
        /// Lädt eine Datei aus dem Internet herunter. Arbeitet asynchron
        /// </summary>
        /// <param name="url">Die Webadresse.</param>
        /// <param name="savePath">Das Zielverzeichnis.</param>
        /// <param name="progress">Eine Variable mit einem Callback, über den der Prozessfortschritt zurückgegeben werden kann.</param>
        /// <returns>Die Task-Variable.</returns>
        public async Task DownloadFileAsync(string url, string savePath, IProgress<int>? progress = null)
        {
            this._taskException = null;
            try
            {
                long fileSize = await GetFileSizeAsync(url);
                // Console.WriteLine($"Dateigröße: {fileSize} Bytes");

                if (fileSize > 0)
                {
                    await DownloadFileAsync(url, savePath, fileSize, progress);
                    // Console.WriteLine("Download abgeschlossen.");
                    progress?.Report(100);
                }
                else
                {
                    throw new Exception("Die Dateigröße ist 0 Byte.");
                    // Console.WriteLine("Die Datei ist leer oder konnte nicht gefunden werden.");
                }
            }
            catch (Exception ex)
            {
                _taskException = ex;
            }

            //int totalSchritte = 100;
            //for (int i = 0; i < totalSchritte; i++)
            //{
            //    await Task.Delay(50);
            //    // Melde den Fortschritt
            //    progress?.Report((i + 1) * 100 / totalSchritte);
            //}
            //progress?.Report(100);
        }

        private Exception? _taskException;

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

        // Methode zum Herunterladen der Datei mit Fortschrittsanzeige
        static async Task DownloadFileAsync(string url, string savePath, long totalSize, IProgress<int>? progress = null)
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
                        byte[] buffer = new byte[8192]; // 8 KB Puffer
                        long totalBytesRead = 0;
                        int bytesRead;

                        // Fortschritt während des Downloads
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            // Berechne und zeige den Fortschritt an
                            int progressPercent = (int)((double)totalBytesRead / totalSize * 100);
                            progress?.Report(progressPercent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Fehler beim Herunterladen der Datei: " + ex.Message);
                }
            }
        }

    }
}
