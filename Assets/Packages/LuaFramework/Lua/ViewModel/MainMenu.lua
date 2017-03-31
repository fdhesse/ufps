
MainMenuViewModel = {    
  
  OnCreate = function(gameob)
    logWarn('OnCreate ViewModel ' .. gameob.name)
    
  end,
  
  OnPve = function()
    --[[
    Game.EnterGame(function()
        
        ClosePanel('MainMenu');
        ClosePanel('Loading');
       
    end);
    --]]
    --TODO:set mission id according user selected    
    LobbyViewModel.MissionId = 0;
    ShowPanel('LobbySceneUI');
  end,
  
  OnPvp = function()
    --[[
    Game.EnterPvp(function()
        ClosePanel('MainMenu');
    end);   
    --]]
    --TODO:set mission id according user selected
    LobbyViewModel.MissionId = 1;
    ShowPanel('LobbySceneUI');    
  end,  
  
   OnLoading = function()
    ShowPanel('Loading');   
  end,  
};
