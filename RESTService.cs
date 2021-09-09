using System;    
using System.Collections.Generic;    
using System.Linq;    
using System.Runtime.Serialization;    
using System.ServiceModel;    
using System.ServiceModel.Web;    
using System.Text;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KWire
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Json,
             BodyStyle = WebMessageBodyStyle.Wrapped,
             UriTemplate = "audiodevices/")]
        [return: MessageParameter(Name = "AudioDevices")]
        AudioDevice GetDevices();

    }

    public class Service : IService
    {
        public AudioDevice GetDevices()
        {
            try
            {

                var response = new AudioDevice();
                response.jsonResponse = JsonConvert.SerializeObject(Core.AudioDevices);
                return response;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public class AudioDevice
    {
       /*
        public int id { get; set; }
       public string name { get; set; }
       public int channels { get; set; }
       */
       //public List<Device> devices { get; set; }

        public string jsonResponse { get; set; }

        
    }


}   

