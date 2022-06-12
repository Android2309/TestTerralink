using TerralinkTest.Models;

namespace TerralinkTest
{
    public sealed class ExternalSystemConnector
    {
        public async Task SendDocuments(IReadOnlyCollection<Document> documents, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            if (documents.Count > 10)
            {
                throw new ArgumentException("Can't send more than 10 documents at once.", nameof(documents));
            }
            // тестовая реализация, просто ничего не делаем 2 секунды
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

    }
}
