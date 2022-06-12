using TerralinkTest;
using TerralinkTest.Models;
using System.Collections.Concurrent;


var documentsQueue = new ConcurrentQueue<Document>();

var connector = new ExternalSystemConnector();

var progress = new Progress<string>(p =>
{
    Console.WriteLine(p);
});

var sender = new DocumentSender(connector, documentsQueue, progress, 5);

sender.StartSendDocumentsAsync(2);

EnqueueDocuments();




//эмуляция постановки в очередь
void EnqueueDocuments()
{
    int i = 0;
    while (true)
    {
        Thread.Sleep(300);
        sender.Enqueue(new Document() { Id = ++i, Body = $"{i}" });

        if (i >= 30)
        {
            sender.Dispose();
            break;
        }
    }
}










