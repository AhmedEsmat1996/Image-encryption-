using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
     class HuffmanNode
    {
       
        public HuffmanNode Left { get; set; }
        public HuffmanNode Right { get; set; }
        public int Value { get; set; }
        public int Count { get; set; }
        public HuffmanNode Clone()
        {
            var obj = (HuffmanNode)this.MemberwiseClone();
            return obj;
        }

    }
    class HuffmanTree
    {
        public HuffmanNode root;

        public HuffmanTree(Dictionary<int, int> counts) //O(N)
        {
            PriorityQueue<HuffmanNode>  priorityQueue = new PriorityQueue<HuffmanNode>();

            foreach (KeyValuePair<int, int> kvp in counts)
            {
                HuffmanNode N = new HuffmanNode();
                N.Value = kvp.Key;
                N.Count = kvp.Value;
                priorityQueue.Enqueue(N, kvp.Value);
            }

            while (priorityQueue.Count > 1)
            {
                HuffmanNode n1 = priorityQueue.Dequeue();
                HuffmanNode n2 = priorityQueue.Dequeue();
                HuffmanNode n3 = new HuffmanNode();
                n3.Left = n2;n3.Right = n1;n3.Count = n1.Count + n2.Count;
                priorityQueue.Enqueue(n3, n3.Count);
            }

            root = priorityQueue.Dequeue();
        }
        
        
        public Dictionary<int, string> CreateEncodings() // O(N)
        {
            Dictionary<int, string> Huffmancodes = new Dictionary<int, string>();
            Create_Encodings_Help(root, "", Huffmancodes);
            return Huffmancodes;
        }

        private void Create_Encodings_Help(HuffmanNode node, string code, Dictionary<int, string> Huffmancodes)
        {
            if (node.Left != null )
            {
                Create_Encodings_Help(node.Left, code + '0', Huffmancodes);
                Create_Encodings_Help(node.Right, code + '1', Huffmancodes);

            }
            else
            {
                Huffmancodes.Add(node.Value, code);
            }
        }
    }

}
