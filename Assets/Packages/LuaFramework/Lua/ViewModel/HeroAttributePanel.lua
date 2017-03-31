HeroModel = {};
local  this = HeroModel;
function HeroModel.Test()
  resMgr:LoadSprite("hero_asset",{"Rick"},function(objs)      
           HeroAttributeViewModel.ShowHero= objs[0];
         HeroAttributeViewModel.UpdateView();
         end)
 end 





local context;
local transform;
HeroAttributeViewModel = {
  Hero = {   };
  level = 0;
  HeroIcon = { };
  ShowHero = "";
  ShowText = "Rick";
  OnCreate = function(gameob,_context)
    logWarn('OnCreate ViewModel ' .. gameob.name)
    transform = gameob.transform;
    context = _context ;
  end,
  
UpdateHero = function()

   local Heros = GameData:GetUserEntity(Game.KingsMan.MessagePack.EntityType.ET_HERO_MODEL);   
    local inventorys = {};
   --[[for  i =1 ,#Heros.InventoryEntGroups,1 do 
      inventorys = Heros.InventoryEntGroups[i].Inventorys
        local propertys = {};
     for j = 1 ,#inventorys,1 do
       propertys = inventorys[j].Propertys;

       local  pType;
       local  pValue;
       for  k =1,#propertys,1 do
         pType = propertys[k].PropType
         pValue = propertys[k].PropValue
          print(pValue[1]);
       end          
     end       
   end
   --]]
 local len = #Heros.InventoryEntGroups;      
   
  -- HeroIcon = Heros.InventoryEntGroups;
 HeroAttributeViewModel.Hero = {};  
 --   HeroIcon = Heros.InventoryEntGroups;
     HeroIcon= {{Heros.InventoryEntGroups[1]
    ,heroIcon,Name
    ,OnChangeHero = function() 
      HeroModel.Test();
     -- HeroAttributeViewModel.ChangeHero();
       HeroAttributeViewModel.ShowText = "Rick";
      
      HeroAttributeViewModel.UpdateView();
      
    end,},{Heros.InventoryEntGroups[2]
    ,heroIcon,Name
    ,OnChangeHero = function()   
      -- HeroAttributeViewModel.ChangeHero();
      HeroAttributeViewModel.ShowText = "Jamie";
    --resMgr:LoadSprite("heroicon_asset",{"headicon01","headicon02","headicon03","headicon04"},function(objs)      
             --   ShowHero= objs[2];
               -- HeroAttributeViewModel.UpdateView();
            -- end)
       HeroAttributeViewModel.UpdateView();
       resMgr:LoadSprite("heroicon_asset",{"headicon02"},function(objs)      
               HeroAttributeViewModel.ShowHero= objs[0];
             HeroAttributeViewModel.UpdateView();
             end)  
    end,},{Heros.InventoryEntGroups[3]
    ,heroIcon,Name
    ,OnChangeHero = function()

         HeroAttributeViewModel.ShowText = "Michonne";
       --  HeroAttributeViewModel.ChangImage("heroicon_asset",{"headicon01","headicon02","headicon03","headicon04"},ShowHero,3);
         HeroAttributeViewModel.UpdateView();
         resMgr:LoadSprite("heroicon_asset",{"headicon03"},function(objs)      
               HeroAttributeViewModel.ShowHero= objs[0];
             HeroAttributeViewModel.UpdateView();
             end)  
         
    end,},{Heros.InventoryEntGroups[4]
    ,heroIcon,Name
    ,OnChangeHero = function()
      
       HeroAttributeViewModel.ShowText = "Pedro";
       HeroAttributeViewModel.UpdateView();
       resMgr:LoadSprite("heroicon_asset",{"headicon04"},function(objs)      
               HeroAttributeViewModel.ShowHero= objs[0];
             HeroAttributeViewModel.UpdateView();
             end)  
       
    end,}};
         resMgr:LoadSprite("heroicon_asset",{"headicon01","headicon02","headicon03","headicon04"},function(objs) 
             for i =1,len,1 do
                 HeroIcon[i].heroIcon = objs[i-1];
                 HeroIcon[i].Name = i;
             end         
             end)
       
        for i= 1 ,len  do
          table.insert(HeroAttributeViewModel.Hero,HeroIcon[i]);     
         end 
   -- print(#tt.InventoryEntGroups[1].Inventorys[1].Propertys[1]);
     -- context:UpdateViewModel();
end,

 
 UpdateView = function()
    context:UpdateViewModel();
end,
  
  
ChooseHero = function()
    
    
 
end,
  
OnChangeWeapon = function()
  ShowPanel('HeroEquipment');
end,


OnReturn = function()
    --logWarn('OnLogin ViewModel')
   -- ShowPanel('LobbySceneUI');
    ClosePanel('HeroAttribute');
    
end,

}