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
    // this appears in the Tools > External Tools submenu in EmuHawk
    [ExternalTool("Sockets")] 
    public sealed class SocketsTool : ToolFormBase, IExternalToolForm
    {

        // Title of window created with Tool (figure out how to remove window eventually)
        protected override string WindowTitleStatic
			=> "Sockets";

        // getting the API container
        public ApiContainer? _maybeAPIContainer { get; set; }

        // Setting APIs to be our APIcontainer 
        private ApiContainer APIs
	        => _maybeAPIContainer!;

        // geting our IP addresses to be used (localhost).
        public static readonly IPHostEntry host_obj = Dns.GetHostEntry("localhost");

        // Getting our localhost IP address.
        public static readonly IPAddress localhost = host_obj.AddressList[1];

        // creation of the IPEndpoint with port 10305
        public static IPEndPoint end_point = new(localhost, 10305);

        // setting up our main socket to bind and listen.
        static Socket main_socket = new(
            end_point.AddressFamily, 
            SocketType.Stream, 
            ProtocolType.Tcp);

        // Socket that is used for our socket server.
        Socket connection_socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

        // default inputs for inputs.
        readonly IReadOnlyDictionary<string, Dictionary<string, bool>> default_input = new Dictionary<string, Dictionary<string, bool>> 
            { 
                {
                    "Game Inputs", 
                    new Dictionary<string, bool>
                    {
                
                    } 
                },


                {
                    "Control Inputs", 
                    new Dictionary<string, bool>
                    {
                        {"Reset", false}
                    } 
                }
            };

        public static IReadOnlyDictionary<string, Dictionary<string, bool>>? inputs;
        public SocketsTool() 
        {
            // setting up the socket server on the initialization of the External tool.
            main_socket.Bind(end_point);
            main_socket.Listen(50);
            connection_socket = main_socket.Accept();
                 
        }
        public override void Restart()
        {
            // executed once after the constructor, and again every time a rom is loaded or reloaded
            APIs.EmuClient.LoadState("KenVsRyu");
            inputs = default_input;
        }

        protected override void UpdateBefore()
        {   
            // gets the frame count from the emulator 
            int frame_count = APIs.Emulation.FrameCount();
            
            byte[] bytes = new byte[1024]; // bytes for receiving over the socket server
            var received_bytes = connection_socket.Receive(bytes); // receiving bytes from socket server
            string json_input = Encoding.UTF8.GetString(bytes, 0, received_bytes); // decoding the bytes 

            // converting our decoded string into our Dictionary to be used as our inputs.
            // If there is for some reason no input we set it to the default inputs.
            inputs = JsonConvert.DeserializeObject<IReadOnlyDictionary<string,  Dictionary<string, bool>>>(json_input) ?? default_input;
            
            // Dummy code for our random inputs
            if (frame_count % 2 == 0) {
                APIs.Joypad.Set(inputs["Game Inputs"]);
            } 
        }

        protected override void UpdateAfter()
        {   
            // setting our memory addresses to be using BigEndian 
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

            // Dictionary that we are sending over the socket server with all important values that we use 
            // in our environment.
            IDictionary<string, int> game_data = new Dictionary<string, int>
            {
                { "P1 Health", Convert.ToInt32(p1_health) },
                { "P2 Health", Convert.ToInt32(p2_health) },
                { "Timer", Convert.ToInt32(game_timer) },
                { "P1 X Coord", Convert.ToInt32(p1_x_coord) },
                { "P2 X Coord", Convert.ToInt32(p2_x_coord) },
                { "P1 Y Coord", Convert.ToInt32(p1_y_coord) },
                { "P2 Y Coord", Convert.ToInt32(p2_y_coord) },
                {"Frame Counter", APIs.Emulation.FrameCount()}
            };
            
            string message = JsonConvert.SerializeObject(game_data);
            connection_socket.Send(Encoding.UTF8.GetBytes(message));
            
            // checking to see if the reset in our inputs is true
            if (inputs!["Control Inputs"]["Reset"]) {

                // reload our ROM
                APIs.EmuClient.OpenRom(@"A:\Coding-Stuff\SF3TS-ai\Sockets\BizHawk\Arcade\sfiii3n.zip");
            }
        }   
    }       
}

