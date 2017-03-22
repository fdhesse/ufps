local transform;

local context;
BackPackViewModel = {
  
  Message = "M1911";
  
  OnCreate = function(gameob,_context)
    transform=gameob.transform;
    context = _context;
  end,
  
  OnInventory = function()
    
   
    context:UpdateViewModel();
    
  end,
  
  
  
  
  
  
  
}
