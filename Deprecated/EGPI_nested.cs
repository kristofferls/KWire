using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lawo.EmberPlusSharp.S101;
using Lawo.Threading.Tasks;
using Lawo.EmberPlusSharp.Model;


namespace KWire
{
    public class EGPI
    {

        private int _id;
        private string _name;
        private bool _state;

        public EGPI(int id, string name)
        {
            _id = id;
            _name = name;
            Logfile.Write("EGPI :: EGPI with ID: " + _id + " and name: " + _name + " created");
            //bool currentState = GetState();
            //string currState = currentState.ToString();

            bool currentState = false; // WORKAROUND until PowerCore has been updated. 

            Logfile.Write("EGPI :: Current state of " + _id + ":" + _name + " is:: " + currentState);
            Logfile.Write("EGPI :: Enabling async monitoring of " + _id + ":" + _name );
            //MonitorState();
            
        }

        public int GPO 
        {
            get { return _id; }
            set { _id = value; }
        
        }

        public string Name 
        {
            get { return _name; }
            set { _name = value; }
        }

        public bool Status 
        {
            get { return _state; }
            private set { _state = value; }
        }

        public bool GetState()
        {
            _state = GetCurrentState();
            if (_state)
            {
                Logfile.Write("EGPI :: EGPI " + _id + " is ON");
                return  true;
            }
            else
            {
                Logfile.Write("EGPI :: EGPI " + _id + " is OFF");
                return false;
            }
        }

        public async Task MonitorState()

        {

            await Task.Run(() =>
            {
                WaitForChange();

                GetState(); //Get the current state of the redlight. 

            }




                ); //end of Task.Run
            //Console.WriteLine("Resuming monitoring task in the background");
            MonitorState();

        }


        private async Task<S101Client> ConnectAsync(string host, int port)
        {
            // Create TCP connection
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port);

            // Establish S101 protocol
            // S101 provides message packaging, CRC integrity checks and a keep-alive mechanism.
            var stream = tcpClient.GetStream();
            return new S101Client(tcpClient, stream.ReadAsync, stream.WriteAsync);
        }

        private bool GetCurrentState()
        {
            bool currentState = false;


            try
            {

                AsyncPump.Run(
                                                async () =>
                                                {
                                                    string EmberProviderIP = Convert.ToString(Config.Ember_IP);
                                                    using (var client = await ConnectAsync(EmberProviderIP, Config.Ember_Port))
                                                    using (var consumer = await Consumer<RubyRoot>.CreateAsync(client))
                                                    {

                                                        var redlightStatus = consumer.Root.Ruby.GPIOs.EGPIO_AUTOCAM.OutputSignals.REDLIGHT.State;
                                                        var valueChanged = new TaskCompletionSource<string>();

                                                        currentState = redlightStatus.Value;


                                                    }
                                                }

                            ); //end of AsyncPump.Run

            }
            catch (Exception error)
            {
                Logfile.Write("EmberConsumer :: " + error);

            }


            return currentState;


        }
        private void WaitForChange()
        {
            try
            {
                AsyncPump.Run(
                                          async () =>
                                          {
                                              string EmberProviderIP = Convert.ToString(Config.Ember_IP);
                                              using (var client = await ConnectAsync(EmberProviderIP, Config.Ember_Port))
                                              using (var consumer = await Consumer<RubyRoot>.CreateAsync(client))
                                              {

                                                  var valueChanged = new TaskCompletionSource<string>();
                                                  var egpiStatus = consumer.Root.Ruby.GPIOs.EGPIO_AUTOCAM.OutputSignals.REDLIGHT.State;
                                                  egpiStatus.PropertyChanged += (s, e) => valueChanged.SetResult(((IElement)s).GetPath());
                                                  string state = valueChanged.Task.ToString();

                                                  //Don't do anything unless there is a change.
                                                  // THIS MIGHT NOT WORK, AS THE CHANGE IS THE ONLY THING THAT IS REPORTED - NOT THE CURRENT STATE. Might need to compare the last known state, and expect it to be the opposite if changed.. 
                                                  var changed = await valueChanged.Task;

                                                  if (changed.Length > 0)
                                                  {
                                                      _state = egpiStatus.Value;
                                                      consumer.Dispose();
                                                      client.Dispose();


                                                  }




                                              }
                                          }

                                           );
            }
            catch (Exception error)
            {
                Logfile.Write("EGPI :: WaitForChange ERROR :: " + error);
                
            }


        }

      

        private bool CreateMonitorTask()
        {
            try
            {
                AsyncPump.Run(
                                        async () =>
                                        {
                                            string EmberProviderIP = Convert.ToString(Config.Ember_IP);
                                            using (var client = await ConnectAsync(EmberProviderIP, Config.Ember_Port))
                                            using (var consumer = await Consumer<RubyRoot>.CreateAsync(client))
                                            {
                                                var valueChanged = new TaskCompletionSource<string>();
                                                var egpiStatus = consumer.Root.Ruby.GPIOs.EGPIO_AUTOCAM.OutputSignals.REDLIGHT.State;
                                                egpiStatus.PropertyChanged += (s, e) => valueChanged.SetResult(((IElement)s).GetPath());
                                                string state = valueChanged.Task.ToString();

                                                var changed = await valueChanged.Task;

                                                consumer.Dispose();
                                                client.Dispose();

                                            }
                                        }

                );
            }

            catch (Exception error)
            {
                Console.WriteLine("Ember.MonitorState: {0}", error);
                return false;
            }

            return true;
        }













    }




}
