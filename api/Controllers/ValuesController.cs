using messages;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace api.Controllers
{
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IBus _bus;

        public ValuesController(IBus bus)
        {
            _bus = bus;
        }

        [HttpGet]
        [Route("simonsnews/{value}")]
        public ActionResult<string> SimonsNews(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _bus.Publish(new SimonsNews {Message = "WEB SERVICE says: " + value});
                return $"Published Simon says 'WEB SERVICE says {value}'";
            }
            return "Tag your message onto the end of ~/api/ to send the message.";
        }

        [HttpGet]
        [Route("simonsorders/{value}")]
        public ActionResult<string> SimonsOrders(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _bus.Send(new SimonsOrders {Message = "WEB SERVICE - Simon's orders are: " + value});
                return $"Sent Simon orders '{value}'";
            }
            return "Tag your message onto the end of ~/api/ to send the message.";
        }
    }
}
