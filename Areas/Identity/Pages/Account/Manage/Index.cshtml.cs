﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NhaKhoaQuangVu.Models;
using System.ComponentModel.DataAnnotations;

namespace NhaKhoaQuangVu.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [Display(Name = "Tên tài khoản")]
        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            [MaxLength(100)]
            [Display(Name = "Họ tên đầy đủ")]
            public string FullName { set; get; }

            [MaxLength(255)]
            [Display(Name = "Địa chỉ")]
            public string Address { set; get; }
        }

        // Nạp thông tin từ User vào Model
        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            Username = userName;
            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                Address = user.Address,
                FullName = user.FullName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Không tải được tài khoản ID = '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Không có tài khoản ID: '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Lỗi cập nhật số điện thoại.";
                    return RedirectToPage();
                }
            }

            // Cập nhật các trường bổ sung
            user.Address = Input.Address;
            user.FullName = Input.FullName;
            await _userManager.UpdateAsync(user);

            // Đăng nhập lại để làm mới Cookie (không nhớ thông tin cũ)
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Hồ sơ của bạn đã cập nhật";
            return RedirectToPage();
        }
    }
}
