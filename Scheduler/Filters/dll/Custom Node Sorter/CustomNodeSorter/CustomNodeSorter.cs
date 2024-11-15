using Microsoft.Hpc.Scheduler.AddInFilter.HpcClient;

namespace NodeSorter
{
    public class NodeSorter : INodeSorter
    {
        public int Compare(string nodeX, string nodeY)
        {
            var result = nodeY.CompareTo(nodeX);
            return result;
        }
    }
}