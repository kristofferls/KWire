using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lawo.EmberPlusSharp.S101;
using Lawo.Threading.Tasks;
using Lawo.EmberPlusSharp.Model;

namespace KWire
{
    public static class EmberConsumer
    {
        private static async Task<S101Client> ConnectAsync(string host, int port)
        {
            // Create TCP connection
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port);

            // Establish S101 protocol
            // S101 provides message packaging, CRC integrity checks and a keep-alive mechanism.
            var stream = tcpClient.GetStream();
            return new S101Client(tcpClient, stream.ReadAsync, stream.WriteAsync);
        }
        public static void PrintTree()
        {
            AsyncPump.Run(
                                      async () =>
                                      {
                                          string EmberProviderIP = Convert.ToString(Config.Ember_IP); //get IP-address from XML - convert to string, as the ConnectAsync expects a string.

                                          using (var client = await ConnectAsync(EmberProviderIP, Config.Ember_Port))
                                          using (var consumer = await Consumer<RubyRoot>.CreateAsync(client))
                                          {
                                            WriteChildren(consumer.Root, 0);

                                          }
                                      });//end of AsyncPump.Run
           


        }

        public static bool CurrentRedlightState()
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

        public static void GetRedlightState()
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
                                                  var redlightStatus = consumer.Root.Ruby.GPIOs.EGPIO_AUTOCAM.OutputSignals.REDLIGHT.State;
                                                  redlightStatus.PropertyChanged += (s, e) => valueChanged.SetResult(((IElement)s).GetPath());
                                                  string state = valueChanged.Task.ToString();

                                                  //Don't do anything unless there is a change.
                                                  //
                                                  var changed = await valueChanged.Task;

                                                  if (changed.Length > 0)
                                                  {
                                                      //Program.RedlightState = redlightStatus.Value;
                                                      consumer.Dispose();
                                                      client.Dispose();


                                                  }




                                              }
                                          }

                                           );
            }
            catch (Exception error)
            {
                Console.WriteLine("GetRedlightState : {0}", error);
            }


        }


        public static bool MonitorState()
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
                                                var redlightStatus = consumer.Root.Ruby.GPIOs.EGPIO_AUTOCAM.OutputSignals.REDLIGHT.State;
                                                redlightStatus.PropertyChanged += (s, e) => valueChanged.SetResult(((IElement)s).GetPath());
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

        public static void SetSource(bool direction)
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
                                                    var EGPI = consumer.Root.Ruby.GPIOs.EGPIO_AUTOCAM.InputSignals.EGPI_SWITCH.State;

                                                    if (direction == true)
                                                    {
                                                        Console.WriteLine("Direction is ON");
                                                        EGPI.Value = true;
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Direction is OFF");
                                                        EGPI.Value = false;
                                                    }

                                                    await consumer.SendAsync();
                                                    Console.WriteLine("Command sent. ");
                                                }
                                            });
            }
            catch (Exception error)
            {
                Console.WriteLine("Ember.SetSource: {0}", error);
            }

        }
        private static void WriteChildren(INode node, int depth)
        {
            var indent = new string(' ', 2 * depth);

            foreach (var child in node.Children)
            {
                var childNode = child as INode;

                if (childNode != null)
                {
                    Console.WriteLine("{0}Node {1}", indent, child.Identifier);
                    WriteChildren(childNode, depth + 1);
                }
                else
                {
                    var childParameter = child as IParameter;

                    if (childParameter != null)
                    {
                        Console.WriteLine("{0}Parameter {1}: {2}", indent, child.Identifier, childParameter.Value);
                    }
                }
            }


        }
    }
}
