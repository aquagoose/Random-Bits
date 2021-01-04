using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKStuffAgain
{
    public static class Program
    {
        private static void Main()
        {
            using (var window = new Window(800, 600, "Learn OpenTK"))
            {
                window.Run(60.0);
            }
        }
    }
}
