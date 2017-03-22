
LobbyLoading = {};
local this = LobbyLoading;
local timer;
function LobbyLoading.TimeUpdateRotate()
  if timer then timer:Stop() end
  
  --if (timer == nil) then
     timer = Timer.New(
      function()
        --print('LobbyModel')     
       -- this.UpdateMatch(missionId);
    
       LobbyLoadingViewModel.Message = LobbyLoadingViewModel.Message-30;
        LobbyLoadingViewModel.UpDateView();
      end, 
      0.1, 
      120,
      false,
      function()
        --print('stop timer')
        --this.CancelMatch(missionId);
      end
      )
  --end

 timer:Start();

end
local context;
LobbyLoadingViewModel = {    
 -- Message = "unknown",
  Message = 5;
  
  OnCreate = function(gameob,_context)
    logWarn('OnCreate ViewModel ' .. gameob.name)
    --transform = gameob.transform;
    context = _context;
   
   
  end,
  UpDateView = function()
    
     context:UpdateViewModel();
  end,
  
  OnOut= function()
    --logWarn('OnLogin ViewModel')
    ClosePanel('LobbyLoading');
  end,
};
