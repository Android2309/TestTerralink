using TerralinkTest.Models;

namespace TerralinkTest.Interfaces
{
    public interface IDocumentsQueue
    {
        void Enqueue(Document document);
    }
}
