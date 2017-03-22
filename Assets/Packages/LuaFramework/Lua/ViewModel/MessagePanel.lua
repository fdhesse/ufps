

MessageViewModel = {    
  Message = "unknown",
  
  OnCreate = function(gameob)
    logWarn('OnCreate ViewModel ' .. gameob.name)
    --transform = gameob.transform;
    
  end,
  
  OnOK = function()
    --logWarn('OnLogin ViewModel')
    ClosePanel('Message');
  end,
};

function MessageBox(message)
  MessageViewModel.Message = message;
  ShowPanel('Message');
end