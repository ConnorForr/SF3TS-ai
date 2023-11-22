using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic; 
using Newtonsoft.Json;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using System.Threading;


namespace Net.MyStuff.Sockets
{
    [ExternalTool("Sockets")] // this appears in the Tools > External Tools submenu in EmuHawk
    public sealed class SocketsTool : ToolFormBase, IExternalToolForm
    {
        protected override string WindowTitleStatic
			=> "Sockets";

        private Label frame_counter_bytes = new() { AutoSize = true };
        public ApiContainer? _maybeAPIContainer { get; set; }
        private ApiContainer APIs
	        => _maybeAPIContainer!;
        public static readonly IPHostEntry host_obj = Dns.GetHostEntry("localhost");
        public static readonly IPAddress localhost = host_obj.AddressList[1];
        public static IPEndPoint end_point = new(localhost, 10305);
        static Socket main_socket = new(
            end_point.AddressFamily, 
            SocketType.Stream, 
            ProtocolType.Tcp);
        Socket connection_socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

        public SocketsTool() 
        {
            ClientSize = new Size(480, 320);
			SuspendLayout();
			Controls.Add(frame_counter_bytes);
			ResumeLayout(performLayout: false);
			PerformLayout();

            main_socket.Bind(end_point);
            main_socket.Listen(50);
            connection_socket = main_socket.Accept();
                 
        }
        public override void Restart()
        {
            // executed once after the constructor, and again every time a rom is loaded or reloaded
        }

        protected override void UpdateBefore()
        {   
            int frame_count = APIs.Emulation.FrameCount()+1;
            connection_socket.Send(Encoding.UTF8.GetBytes(frame_count.ToString()));
            frame_counter_bytes.Text = frame_count.ToString();

            /*if (frame_count % 2 == 0) {
                byte[] bytes = new byte[1024];
                var received_bytes = connection_socket.Receive(bytes);
                string json_input = Encoding.UTF8.GetString(bytes, 0, received_bytes);
                IReadOnlyDictionary<string, bool>? inputs = JsonConvert.DeserializeObject<IReadOnlyDictionary<string, bool>>(json_input);

                APIs.Joypad.Set(inputs);
            } */
        }

        protected override void UpdateAfter()
        {        
            APIs.Memory.SetBigEndian();

            // Player 1 Health 
            uint p1_health = APIs.Memory.ReadU8(0x068D0B, "sh2 : ram : 0x2000000-0x207FFFF");

            // Player 2 Health
            uint p2_health = APIs.Memory.ReadU8(0x0691A3, "sh2 : ram : 0x2000000-0x207FFFF");

            // Timer
            uint game_timer = APIs.Memory.ReadU8(0x011377, "sh2 : ram : 0x2000000-0x207FFFF");

            // X coord p1
            uint p1_x_coord = APIs.Memory.ReadU16(0x06D498, "sh2 : ram : 0x2000000-0x207FFFF");

            // X coord P2
            uint p2_x_coord = APIs.Memory.ReadU16(0x069168, "sh2 : ram : 0x2000000-0x207FFFF");

            // Y coord P1
            uint p1_y_coord = APIs.Memory.ReadU8(0x068CD5, "sh2 : ram : 0x2000000-0x207FFFF");

            // Y coord P2
            uint p2_y_coord = APIs.Memory.ReadU8(0x06916D, "sh2 : ram : 0x2000000-0x207FFFF");

            IDictionary<string, int> game_data = new Dictionary<string, int>
            {
                { "P1 Health", Convert.ToInt32(p1_health) },
                { "P2 Health", Convert.ToInt32(p2_health) },
                { "Timer", Convert.ToInt32(game_timer) },
                { "P1 X Coord", Convert.ToInt32(p1_x_coord) },
                { "P2 X Coord", Convert.ToInt32(p2_x_coord) },
                { "P1 Y Coord", Convert.ToInt32(p1_y_coord) },
                { "P2 Y Coord", Convert.ToInt32(p2_y_coord) },
                {"Frame Counter", APIs.Emulation.FrameCount()+1}
            };
            
            string message = JsonConvert.SerializeObject(game_data);

            connection_socket.Send(Encoding.UTF8.GetBytes(message));
        }   
    }       
}

