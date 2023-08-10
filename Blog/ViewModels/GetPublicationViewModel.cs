using Blog.Models;

namespace Blog.ViewModels
{
    public class GetPublicationViewModel
    {
        public Publication Publication { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
