using Blog.Data;
using Blog.Interfaces;
using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Repository
{
    public class SubscriberRepository : ISubscriber
    {
        private readonly ApplicationContext _context;

        public SubscriberRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<bool> IsSubscribe(string email)
        {
            return await _context.Subscribers.AnyAsync(e => e.Email.Equals(email));
        }

        public async Task Subscribe(Subscriber subscriber)
        {
            await _context.Subscribers.AddAsync(subscriber);
            await _context.SaveChangesAsync();
        }
    }
}
