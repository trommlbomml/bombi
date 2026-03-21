using Microsoft.AspNetCore.Mvc;

namespace Bombi.ServerInstance;

[ApiController]
[Route("game-instance")]
public sealed class GameInstanceController(IGameInstanceService service) : ControllerBase
{
    [HttpPost]
    public IActionResult JoinInstanceAsAdmin([FromBody]UserViewModel viewModel)
    {
        var token = service.StartCreateInstance(viewModel.Name);

        return Ok(
            new
            {
                Token = token
            }
        );
    }
    
    [HttpPut("{instanceId:guid}")]
    public IActionResult JoinInstance(Guid instanceId, [FromBody]UserViewModel viewModel)
    {
        var token = service.StartJoinInstance(instanceId, viewModel.Name);

        return Ok(
            new
            {
                Token = token
            });
    }

    public sealed class UserViewModel
    {
        public required string Name { get; set; }
    }
}