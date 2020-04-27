using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using Core.Entities.Identity;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManeger;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManeger, 
            SignInManager<AppUser> signInManager, 
            ITokenService tokenService, 
            IMapper mapper)
        {
            _signInManager = signInManager;
            _userManeger = userManeger;
            _tokenService = tokenService;
            _mapper = mapper;
        }


        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetGurrentUser()
        {
            
            var user = await _userManeger.FindByEmailFromClaimsPrinciple(HttpContext.User);
            return new UserDto
            {
                Email = user.Email, 
                Token = _tokenService.CreateToken(user),
                DisplayName = user.DisplayName
            };
        }

        [HttpGet("emailExists")]
        public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
        {
            return await _userManeger.FindByEmailAsync(email) != null;
        }

        [Authorize]
        [HttpGet("address")]
        public async Task<ActionResult<AddressDto>> GetUserAddress()
        {
            var user = await _userManeger.FindByUserByClaimsPrincipleWithAddressAsync(HttpContext.User);
            return _mapper.Map<Address, AddressDto>(user.Address);
        }

        [Authorize]
        [HttpPut("address")]
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto address)
        {
            var user = await _userManeger.FindByUserByClaimsPrincipleWithAddressAsync(HttpContext.User);
            user.Address =  _mapper.Map<AddressDto, Address>(address);
            var result = await _userManeger.UpdateAsync(user);
            if(result.Succeeded) return Ok(_mapper.Map<Address, AddressDto>(user.Address));
            return BadRequest("Problem updating the user");
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto  loginDto)
        {
            var user = await _userManeger.FindByEmailAsync(loginDto.Email);
            if (user == null) return Unauthorized(new ApiResponse(401));
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded) return Unauthorized(new ApiResponse(401));
            return new UserDto
            {                
                Email = user.Email, 
                Token = _tokenService.CreateToken(user),
                DisplayName = user.DisplayName
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName, 
                Email = registerDto.Email, 
                UserName = registerDto.Email
            };

            var result = await _userManeger.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400));

            return new UserDto
            {
                DisplayName = user.DisplayName, 
                Token = _tokenService.CreateToken(user), 
                Email = user.Email
            };
        }        



    }
}