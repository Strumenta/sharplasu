using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Strumenta.Sharplasu.Traversing
{
    public static class StackExtensions
    {
        public static void PushAll<T>(this Stack<T> stack, IEnumerable<T> elements)
        {
            foreach (var item in elements.Reverse())
            {
                stack.Push(item);
            };
        }
    }
}
