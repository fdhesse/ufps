
local context;

CombatPanelViewModel = {    
  Win = false,
  
  SetWin = function(win)
    CombatPanelViewModel.Win = win;
    context:UpdateViewModel();
  end,
  
  OnCreate = function(gameob, _context)
    logWarn('OnCreate ViewModel ' .. gameob.name)
    context = _context;
  end,
  
  OnExit = function()
    ClosePanel('Combat');
    GameAPI.ExecuteEvent("ExitCombat");
    GameAPI.ExecuteEvent("LoadLobby");
  end,
  
  OnRetry = function()
    ClosePanel('Combat');
    GameAPI.ExecuteEvent("ExitCombat");
    GameAPI.ExecuteEvent("LoadLobby");
  end,
};
