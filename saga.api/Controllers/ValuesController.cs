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
        [Route("createcustomer/{name}")]
        public ActionResult<string> LookFor(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _bus.Send(new OnboardCustomer {CustomerName = name});
                return $"Creating customer '{name}'";
            }
            return "Tag the name of the customer onto the end of '~/api/createcustomer/'";
        }
    }
}
