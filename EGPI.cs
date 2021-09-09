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
            GetState();
            //string currState = currentState.ToString();

            

            Logfile.Write("EGPI :: Current state of " + this._id + ":" + this._name + " is:: " + this._state);
            Logfile.Write("EGPI :: Enabling async monitoring of " + this._id + ":" + this._name);
            MonitorState();

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

        public void GetState()
        {
           GetCurrentState();
            if (_state == true)
            {
                Logfile.Write("EGPI :: " + this._name + " is ON / TRUE");
                
            }
            else if (_state == false)
            {
                Logfile.Write("EGPI :: " + this._name + " is OFF / FALSE");
                
            }
        }

        public async Task MonitorState()
        {

            await Task.Run(() =>
            {
                WaitForChange();
                //GetState(); //Get the current state of the redlight. 


            }

            ); //end of Task.Run
            //Console.WriteLine("Resuming monitoring task in the background");
            
            MonitorState();

        }

        private void WaitForChange ()
        {
           
           AsyncPump.Run(
           async () =>
           {
               using (var client = await EmberConsumer.ConnectAsync(Config.Ember_IP,Config.Ember_Port ))
               using (var consumer = await Consumer<PowerCoreRoot>.CreateAsync(client))
               {
                   INode root = consumer.Root;

                   // Navigate to the parameter we're interested in.

                   var mixer = (INode)root.Children.First(c => c.Identifier == "Ruby"); //Defined by Lawo / OnAirDesigner
                   var gpios = (INode)mixer.Children.First(c => c.Identifier == "GPIOs"); //Defined by Lawo / OnAirDesigner
                   var egpio_autocam = (INode)gpios.Children.First(c => c.Identifier == Config.Ember_ProviderName); //Set in OnAirDesigner, and is red from setting.xml.
                   var output_signals = (INode)egpio_autocam.Children.First(c => c.Identifier == "Output Signals");
                   var gpo = (INode)output_signals.Children.First(c => c.Identifier == this._name); //Comes from settings.xml, and needs to correspond EXACTLY with what is defined in OnAirDesigner. 
                   var state = gpo.Children.First(c => c.Identifier == "State");


                   /*
                   var sapphire = (INode)root.Children.First(c => c.Identifier == "Sapphire");
                   var sources = (INode)sapphire.Children.First(c => c.Identifier == "Sources");
                   var fpgm1 = (INode)sources.Children.First(c => c.Identifier == "FPGM 1");
                   var fader = (INode)fpgm1.Children.First(c => c.Identifier == "Fader");
                   var positionParameter = fader.Children.First(c => c.Identifier == "Position");
                   */
                   

                   var valueChanged = new TaskCompletionSource<string>();
                   //var changed = await valueChanged.Task;
                   state.PropertyChanged += (s, e) => valueChanged.SetResult(((IElement)s).GetPath()) ;
                   
                   Logfile.Write("EGPI :: ID: " + this._id + " NAME: " + this._name + " with path " + await valueChanged.Task + " has changed.");
                   

                   var stateParameter = state as IParameter;
                   _state = Convert.ToBoolean(stateParameter.Value);

                   if (_state == true) 
                   {
                       Logfile.Write("EGPI :: " + this._name + " is ON / TRUE");
                   }
                   else if (_state == false)
                   {
                       Logfile.Write("EGPI :: " + this._name + " is OFF / FALSE");
                   }
               }
           });
        }

        public void GetCurrentState() 
        {
            

            AsyncPump.Run(
           async () =>
           {
               using (var client = await EmberConsumer.ConnectAsync(Config.Ember_IP, Config.Ember_Port))
               using (var consumer = await Consumer<PowerCoreRoot>.CreateAsync(client))
               {
                   INode root = consumer.Root;

                   // Navigate to the parameter we're interested in.

                   var mixer = (INode)root.Children.First(c => c.Identifier == "Ruby"); //Defined by Lawo / OnAirDesigner
                   var gpios = (INode)mixer.Children.First(c => c.Identifier == "GPIOs"); //Defined by Lawo / OnAirDesigner
                   var egpio_autocam = (INode)gpios.Children.First(c => c.Identifier == Config.Ember_ProviderName); //Set in OnAirDesigner, and is red from setting.xml.
                   var output_signals = (INode)egpio_autocam.Children.First(c => c.Identifier == "Output Signals");
                   var gpo = (INode)output_signals.Children.First(c => c.Identifier == this._name); //Comes from settings.xml, and needs to correspond EXACTLY with what is defined in OnAirDesigner. 
                   var state = gpo.Children.First(c => c.Identifier == "State");

                    //Read current state of variable. The return is cast as a string, so in order to use it elsewhere, it's cast to bool by Convert.ToBoolean. 
                   var stateParameter = state as IParameter;
                   _state = Convert.ToBoolean(stateParameter.Value);

                   
                      //_state = stateParameter.Value.ToString();
                   

                   

                   // DISPOSE OF CONNECTION! 
                   /*
                   var valueChanged = new TaskCompletionSource<string>();
                   state.PropertyChanged += (s, e) => valueChanged.SetResult(((IElement)s).GetPath());
                   Console.WriteLine("Waiting for the parameter to change...");
                   Console.WriteLine("A value of the element with the path {0} has been changed.", await valueChanged.Task);

                   */
               }
           });
           
        }
        
       













    }




}
