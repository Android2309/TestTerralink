using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using TerralinkTest.Interfaces;
using TerralinkTest.Models;

namespace TerralinkTest
{
    public class DocumentSender : IDocumentsQueue, IDisposable
    {
        /// <summary>
        /// количество отправляемых документов в одном запросе
        /// </summary>
        int pageCount;

        ExternalSystemConnector connector;

        ConcurrentQueue<Document> documents;

        IProgress<string> progress;

        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken;

        public DocumentSender(ExternalSystemConnector connector, ConcurrentQueue<Document> documents, 
            IProgress<string> progress, int pageCount = 10)
        {
            this.pageCount = pageCount;
            this.connector = connector;
            this.documents = documents;
            this.progress = progress;

            cancellationToken = cancelTokenSource.Token;
        }

        public void Enqueue(Document document)
        {
            ValidateDocument(document);

            documents.Enqueue(document);

            progress?.Report($"Документ ИД : {document.Id} добавлен в очередь");
        }

        /// <summary>
        /// Старт отправки документов
        /// </summary>
        /// <param name="sendPeriod"> частота отправки запросов в секундах</param>
        public async void StartSendDocumentsAsync(int sendPeriod)
        {
            if(sendPeriod <= 0)
                throw new ArgumentOutOfRangeException("Аргумент sendPeriod должен быть больше 0");

            while (true)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await Task.Delay(TimeSpan.FromSeconds(sendPeriod), cancellationToken);

                    var documentsToSend = GetDocumentsToSend();

                    if (!documentsToSend.Any())
                        continue;

                    await connector.SendDocuments(documentsToSend, cancellationToken);

                    foreach (var document in documentsToSend)
                        progress?.Report($"Документ ИД : {document.Id} отправлен");
                }
                catch (TaskCanceledException)
                {
                    progress?.Report($"Отправка документов остановлена");
                }
                catch(Exception ex)
                {
                    progress?.Report($"{ex.Message}");
                }
            }
        }

        private List<Document> GetDocumentsToSend()
        {
            var documentsToSend = new List<Document>();

            while (documents.Count > 0 && documentsToSend.Count < pageCount)
            {
                var document = new Document();

                if (!documents.TryDequeue(out document))
                    throw new Exception("Ошибка удаления из очереди");

                documentsToSend.Add(document);
            }

            return documentsToSend;
        }


        private void ValidateDocument(Document document)
        {
            var context = new ValidationContext(document);

            var validationResult = new List<ValidationResult>();

            if (!Validator.TryValidateObject(document, context, validationResult, true))
            {
                string errors = "";

                foreach (var error in validationResult)
                    errors += $"{error} \n";

                throw new Exception($"{errors}");
            }
        }

        public void Dispose()
        {
            cancelTokenSource.Cancel();

            cancelTokenSource.Dispose();

            documents.Clear();
        }
    }
}
