
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
    ShowPanel('LobbySceneUI');
  end,
  
  OnPvp = function()
    Game.EnterPvp(function()
        ClosePanel('MainMenu');
    end);   
  end,  
  
   OnLoading = function()
    ShowPanel('Loading');   
  end,  
};
