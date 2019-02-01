using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
namespace SlicingXDO
{
    class Program
    {
        static void Main(string[] args)
        {
            var window = new GameWindow(800, 600);
            var game = new Game(window);

            window.Run();
        }
    }
}
