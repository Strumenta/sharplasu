using Strumenta.Sharplasu.Model;

namespace Strumenta.Sharplasu.CodeGeneration
{
    /// <summary>
    /// Interface for printing a single node type.
    /// </summary>
    public interface INodePrinter
    {
        void Print(PrinterOutput output, Node ast);
    }
}