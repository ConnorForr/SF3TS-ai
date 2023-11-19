using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;


namespace Net.MyStuff.Sockets
{
    [ExternalTool("Sockets")] // this appears in the Tools > External Tools submenu in EmuHawk
    public sealed class SocketsTool : ToolFormBase, IExternalToolForm
    {
        protected override string WindowTitleStatic
			=> "Sockets";

        public SocketsTool() 
        {
            IPHostEntry host_obj = Dns.GetHostEntry("localhost");
            IPAddress localhost = host_obj.AddressList[1];

            IPEndPoint end_point = new(localhost, 10305);

            using Socket listener_socket = new(
                end_point.AddressFamily, 
                SocketType.Stream, 
                ProtocolType.Tcp);

            listener_socket.Bind(end_point);
            listener_socket.Listen(50);
            
            Socket new_socket = listener_socket.Accept();
            Console.WriteLine("connected");
        }
        public override void Restart()
        {
            // executed once after the constructor, and again every time a rom is loaded or reloaded
        }

        protected override void UpdateAfter()
        {
            // executed after every frame (except while turboing, use FastUpdateAfter for that)
        }
    }
}
