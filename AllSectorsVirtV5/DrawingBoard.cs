using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AllSectorsVirtV5
{
    public class DrawingBoard : Hub
    {
        private const int BoardWidth = 300;
        private const int BoardHeight = 300;
        private static int[,] _buffer = GetEmptyBuffer();
        public Task BroadcastPoint(int x, int y)
        {
            if (x < 0) { x = 0; }
            if (x >= BoardWidth) { x = BoardWidth - 1; }
            if (y < 0) { y = 0; }
            if (y >= BoardHeight) { y = BoardHeight - 1; }

            int color = 0;
            int.TryParse(Clients.Caller.color, out color);
            _buffer[x, y] = color;
            return Clients.Others.DrawPoint(x, y, Clients.Caller.color);

        }

        public Task BroadcastClear()
        {
            _buffer = GetEmptyBuffer();
            return Clients.Others.Clear();
        }

        public override Task OnConnected()
        {
            return Clients.Caller.Update(_buffer);
        }

        public static int[,] GetEmptyBuffer()
        {
            var buffer = new int[BoardWidth, BoardHeight];
            return buffer;
        }
    }
}
