using Microsoft.Hpc.Scheduler.AddInFilter.HpcClient;

namespace NodeSorter
{
    public class NodeSorter : INodeSorter
    {
        // This method is used to determine the order that nodes should be used for jobs and tasks
        // The method should return -1 if nodeX should be used before nodeY
        // The method should return 1 if nodeX should be used after nodeY
        // A tie breaker should be used if nodeX and nodeY have the same name (this method should not return 0)

        // In this case nodes are sorted in reverse lexicographical order
        // So if the available nodes are: nodeA, nodeB, nodeC
        // They will be used in the order: nodeC, nodeB, nodeA
        public int Compare(string nodeX, string nodeY)
        {
            var result = nodeY.CompareTo(nodeX);
            result = result == 0 ? 1 : result;
            return result;
        }
    }
}