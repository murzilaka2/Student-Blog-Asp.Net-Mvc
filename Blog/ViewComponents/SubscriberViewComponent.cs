using Blog.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Blog.ViewComponents
{
    public class SubscriberViewComponent:ViewComponent
    {
        private readonly ISubscriber _subscribers;

        public SubscriberViewComponent(ISubscriber subscribers)
        {
            _subscribers = subscribers;
        }
        public IViewComponentResult Invoke()
        {
            return View("Subscriber");
        }
    }
}
