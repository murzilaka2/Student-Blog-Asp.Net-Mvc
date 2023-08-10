using Blog.Interfaces;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Blog.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MembershipController : Controller
    {
        private readonly IMembership _membership;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public MembershipController(IMembership membership, IHttpContextAccessor httpContextAccessor)
        {
            _membership = membership;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allMemberships = await _membership.GetAllMembershipsAsync();
            return View(allMemberships);
        }

        [HttpGet]
        public async Task<IActionResult> Generate()
        {
            string code = Guid.NewGuid().ToString();
            string link = HttpContext.Request.Scheme+ "://" + _httpContextAccessor.HttpContext.Request.Host.Value + "/register?code=" + code;
            var membership = new Membership
            {
                CreatedDate = DateTime.Now,
                IsEnable = true,
                Code = code,
                Link = link
            };
            await _membership.AddMembershipAsync(membership);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(int membershipId)
        {
            var currentMembership = await _membership.GetMembershipAsync(membershipId);
            if (currentMembership != null)
            {
                await _membership.DeleteMembershipAsync(currentMembership);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
