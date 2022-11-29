using Microsoft.AspNetCore.Mvc;
using Shop.Api.Entities;
using Shop.Api.Repositories;
using AutoMapper;
using Shop.Api.Enums;
using Shop.Api.MiddleWare;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/v1/auth/")]
public class UserController : ControllerBase
{
    private readonly IUserRepository repository;
    private readonly ILogger<ShopItemsController> logger;
    private IMapper mapper;

    public UserController(ILogger<ShopItemsController> logger, IMapper mapper, IUserRepository repository)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.repository = repository;
    }

    [HttpPost]
    [Route("sign-up")]
    public async Task<ActionResult<IEnumerable<UserModel>>> SignUpAsync([FromBody] UserSignUp userSignUp)
    {       
        try{
            UserModel user = new(){
                Id = Guid.NewGuid(),
                Username = userSignUp.Username,
                Password = userSignUp.Password,
                Role = RoleEnum.User,
                CreatedDate = DateTimeOffset.UtcNow,
            }; 

            var userCreated = await repository.SignUp(user);

            if(userCreated==null) return NoContent();
            
            return BadRequest(new{error= "User with such username already exist"});
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<AuthenticateDataDto>> LoginAsync([FromBody] UserLogin userLogin)
    {       
        try{
            var response = await repository.Login(userLogin);

            if(response==null) return NotFound(new{error = "InCorrect Password or Username"});
            var userDto = this.mapper.Map<UserModelDto>(response.User);

            var authenticateDataDto = new AuthenticateDataDto(){
                User=userDto,
                AccessToken=response.Token
            };

            return Ok(new{data = authenticateDataDto});
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }
  
    [HttpPatch]
    [Authorize]
    [Route("role/{id}")]
    public async Task<ActionResult> ChangeUserRoleAsync(Guid id, [FromBody] ChangeUserRoleDto changeRole)
    {       
        try{

            UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
            var isUserAuthorized = user.Role == RoleEnum.SuperAdmin || user.Role == RoleEnum.Admin || changeRole.newRole != RoleEnum.SuperAdmin; 
            if(!isUserAuthorized) return NotFound(new{error = "User with such id doesnt exist, or you cant change his role"});
            
            var response = await repository.ChangeUserRole(id, changeRole.newRole, user.Role);

            if(response==0) return NotFound(new{error = "User with such id doesnt exist, or you cant change his role"});

            return NoContent();
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

    [HttpGet]
    [Authorize]
    [Route("userInfo")]
    public async Task<ActionResult<UserModelDto>> GetUserInfoAsync()
    {       
        try{

            UserModel user = (UserModel)Request.HttpContext.Items["User"]!;

            var userDto = this.mapper.Map<UserModelDto>(user);

            return Ok(new{userInfo = userDto});
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

}