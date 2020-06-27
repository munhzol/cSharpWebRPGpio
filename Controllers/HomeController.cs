using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using testMvc.Models;

using System.Device.Gpio;
using System.Threading;
using Microsoft.AspNetCore.Http;

using System.Net;
using System.IO;

namespace testMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private static int _gPioPinBlueLight = 17;
        private static int _gPioPinRedLight = 2;
        private static GpioController _controller;
        private static bool _run = true;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            
            string tempType = ((HttpContext.Session.GetString("tempType")!=null)?HttpContext.Session.GetString("tempType"):"C");
            HttpContext.Session.SetString("tempType",tempType);
            ViewBag.tempType = tempType;

            ViewBag.temp = celsus();

            int green = (int)((HttpContext.Session.GetInt32("green")!=null)?HttpContext.Session.GetInt32("green"):0);
            HttpContext.Session.SetInt32("green",(int)green);
            ViewBag.green = green;

            int red = (int)((HttpContext.Session.GetInt32("red")!=null)?HttpContext.Session.GetInt32("red"):0);
            HttpContext.Session.SetInt32("red",(int)green);
            ViewBag.red = red;

            int motor = (int)((HttpContext.Session.GetInt32("motor")!=null)?HttpContext.Session.GetInt32("motor"):0);
            HttpContext.Session.SetInt32("motor",(int)motor);
            ViewBag.motor = motor;                     


            return View();
        }


        [HttpGet("light/{id}/{status}")]
        public IActionResult ChangeLight(string id, int status) {
            light(id,status);
            return Redirect("/");
        }


        [HttpGet("motor/{gradus}")]
        public IActionResult ChangeLight(int gradus) {

            
            string html = string.Empty;
            string url = $"http://192.168.1.169/rotate?val={gradus}";            

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            HttpContext.Session.SetInt32("motor",gradus);

            return Redirect("/");
        }


        public IActionResult Off()
        {
            _controller = new GpioController();

            _controller.OpenPin(_gPioPinBlueLight, PinMode.Output);
            _controller.OpenPin(_gPioPinRedLight, PinMode.Output);

            _controller.Write(_gPioPinBlueLight, PinValue.High);
            _controller.Write(_gPioPinRedLight, PinValue.High);


            return View("Index");
        }

        public IActionResult On()
        {
            _controller = new GpioController();

            _controller.OpenPin(_gPioPinBlueLight, PinMode.Output);
            _controller.OpenPin(_gPioPinRedLight, PinMode.Output);

            _controller.Write(_gPioPinBlueLight, PinValue.Low);
            _controller.Write(_gPioPinRedLight, PinValue.Low);


        // static int _switchPin = 17;
        // if (_controller.Read(_switchPin) == PinValue.High)
        // {
        //     // button pressed
        //     value = 0;
        // }

            return View("Index");
        }

        public void light(string id, int status)
        {
            _controller = new GpioController();
            int powerOnRedLightInMilliSec = 1000;

            if(id=="green"){
                _controller.OpenPin(_gPioPinBlueLight, PinMode.Output);
                if(status==1)
                    _controller.Write(_gPioPinBlueLight, PinValue.High);
                else
                    _controller.Write(_gPioPinBlueLight, PinValue.Low);
                HttpContext.Session.SetInt32("green",status);
            }
            else {
                _controller.OpenPin(_gPioPinRedLight, PinMode.Output);
                if(status==1)
                    _controller.Write(_gPioPinRedLight, PinValue.High);
                else
                    _controller.Write(_gPioPinRedLight, PinValue.Low);
                HttpContext.Session.SetInt32("red",status);
            }

            // Thread.Sleep (powerOnRedLightInMilliSec);
        }

        
        public string celsus()
        {
            string html = string.Empty;
            string url = @"http://192.168.1.169/temp";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            html = html.Replace("{ \"temp\": \"","");
            html = html.Replace("\" }","");
            float celsus = float.Parse(html);

            return html;

        }

        
    }
}
