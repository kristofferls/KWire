using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace KWire
{
    public class Kwire_Service
    {
        private readonly Timer _timer;

        public Kwire_Service()

        {
            Core.Setup(); // Call setup-method to initiate program;
            _timer = new Timer(30) { AutoReset = true }; // Timer used to call the BroadcastToAutocam function. 
            _timer.Elapsed += TimerElapsed;

            Logfile.Write("KWire Service :: Constructor done");
            

        }

        private void TimerElapsed(object sender, ElapsedEventArgs e) 
        {
            Core.BroadcastToAutoCam();
        
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop() 
        {
            _timer.Stop();
        }

    }
}
