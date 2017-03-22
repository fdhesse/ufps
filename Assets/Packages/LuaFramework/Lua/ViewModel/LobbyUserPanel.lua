LobbyUserViewModel = {
  
  
  OnCreate = function(gameob)
    logWarn('OnCreate ViewModel ' .. gameob.name)
    --transform = gameob.transform;
    
  end,
  OnShowAttribute= function()
    
    ShowPanel("HeroAttribute");
    ClosePanel("LobbySceneUI");
  end
  
  
  };