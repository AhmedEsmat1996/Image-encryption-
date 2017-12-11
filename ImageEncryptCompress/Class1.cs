using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman1
{
     class PriorityQueue<T>
    {

        private  SortedDictionary<int, Queue<T>> sd = new SortedDictionary<int, Queue<T>>();

        public int Count { get; set; }

        public void Enqueue(T item, int priority)
        {
            ++Count;
            if (!sd.ContainsKey(priority)) sd[priority] = new Queue<T>();
            sd[priority].Enqueue(item);
        }

        public T Dequeue()
        {
            Count--;
           KeyValuePair<int,Queue<T>> item = sd.First();
            if (item.Value.Count == 1) sd.Remove(item.Key);
            return item.Value.Dequeue();
        }
    }
    internal class HuffmanNode
    {
        public HuffmanNode Parent { get; set; }
        public HuffmanNode Left { get; set; }
        public HuffmanNode Right { get; set; }
        public int Value { get; set; }
        public int Count { get; set; }

    }
    class HuffmanTree
    {
        private  HuffmanNode root;

        public HuffmanTree(Dictionary<int, int> counts)
        {
            PriorityQueue<HuffmanNode>  priorityQueue = new PriorityQueue<HuffmanNode>();

            foreach (KeyValuePair<int, int> kvp in counts)
            {
                priorityQueue.Enqueue(new HuffmanNode { Value = kvp.Key, Count = kvp.Value }, kvp.Value);
            }

            while (priorityQueue.Count > 1)
            {
                HuffmanNode n1 = priorityQueue.Dequeue();
                HuffmanNode n2 = priorityQueue.Dequeue();
                HuffmanNode n3 = new HuffmanNode { Left = n2, Right = n1, Count = n1.Count + n2.Count }; 
                n1.Parent = n3;
                n2.Parent = n3;
                priorityQueue.Enqueue(n3, n3.Count);
            }

            root = priorityQueue.Dequeue();
        }

        public Dictionary<int, string> CreateEncodings()
        {
            Dictionary<int, string> encodings = new Dictionary<int, string>();
            Encode(root, "", encodings);
            return encodings;
        }

        private void Encode(HuffmanNode node, string path, Dictionary<int, string> encodings)
        {
            if (node.Left != null )
            {
                Encode(node.Left, path + "0", encodings);
                Encode(node.Right, path + "1", encodings);

            }
            //if (node.Right!=null)
            //{
            //    
            //}
            else
            {
                encodings.Add(node.Value, path);
            }
        }
    }

}
