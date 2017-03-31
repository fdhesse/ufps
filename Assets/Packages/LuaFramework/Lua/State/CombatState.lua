
CombatState = 
{
  
}
local this = CombatState;
local timer;

function CombatState.Enter()
  PreloadPanel('Combat');

  --start heartbeat timer
  if timer then timer:Stop() end  
  timer = Timer.New(
    function()
      --print('LobbyModel')     
      LobbyModel.HeartBeat();  
    end, 
    5, 
    6000, 
    false,
    function()
      --print('stop timer')
      --this.CancelMatch(missionId);
    end
  )
  timer:Start();
end

function CombatState.Exit()
  if timer then timer:Stop() end
end
