﻿using FriendFace.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FriendFace.Data;
using FriendFace.ViewModels;
using Microsoft.Extensions.Logging;
using System.Linq;
using FriendFace.Services.DatabaseService;
using Microsoft.EntityFrameworkCore;

namespace FriendFace.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // !! here we still need to find the user that is logged in, and handle if no user is logged in !!
            User loggedInUser = UserQueryService.getUser(_context, 1);
            HomeIndexViewModel homeIndexViewModel = new HomeIndexViewModel()
            {
                User = loggedInUser,
                PostsInFeed = PostQueryService.getLatestPostsFromFollowingUserIDs(_context,
                    UserQueryService.getFollowingUserIds(loggedInUser))
            };

            return View(homeIndexViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> LikePost([FromBody] int postId)
        {
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            var user = UserQueryService.getUser(_context, 1);

            var existingLike =
                await _context.UserLikesPosts.SingleOrDefaultAsync(like =>
                    like.UserId == user.Id && like.PostId == postId);

            if (existingLike != null)
            {
                // If a like by the user already exists, remove it
                _context.UserLikesPosts.Remove(existingLike);
            }
            else
            {
                // If no like by the user exists, add a new one
                var like = new UserLikesPost
                {
                    UserId = user.Id,
                    PostId = postId
                };

                _context.UserLikesPosts.Add(like);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetPostLikes([FromQuery] int postId)
        {
            var post = PostQueryService.getPostFromId(_context, postId);

            if (post == null)
            {
                return NotFound();
            }

            var user = UserQueryService.getUser(_context, 1);

            var isLiked = post.Likes.Any(like => like.UserId == user.Id);

            var likeCount = post.Likes.Count;

            return Json(new { likeCount = likeCount, isLiked = isLiked });
        }
    }
}