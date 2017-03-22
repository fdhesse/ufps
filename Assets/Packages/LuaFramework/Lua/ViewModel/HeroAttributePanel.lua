HeroAttributeViewModel = {

  OnCreate = function(gameob)
    logWarn('OnCreate ViewModel ' .. gameob.name)
    --transform = gameob.transform;
    
  end,
  
  OnReturn = function()
    --logWarn('OnLogin ViewModel')
    ShowPanel('LobbySceneUI');
    ClosePanel('HeroAttribute');
    
  end,

}