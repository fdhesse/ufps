
PvpPlayer = {};

local this = PvpPlayer;

function PvpPlayer.Init(player, isLocal)
  print('Pvp.Init');
  
      --resMgr:LoadPrefab("weapons", {"Pistol 1911", "RPG"}, function(weapons)      
        --local pistol = UnityEngine.Object.Instantiate(weapons[0]); 
        --local rpg = UnityEngine.Object.Instantiate(weapons[1]); 
        
        resMgr:LoadPrefab("player", {"Player Camera"}, function(objs)            
           local camera = UnityEngine.Object.Instantiate(objs[0]); 
           --local player = UnityEngine.Object.Instantiate(objs[0]); 
           local s = player:GetComponent('PlayerAPI');
           s:Init(camera, isLocal);
           
           --local p = player:GetComponent('PlayerAtts');
           --p.weapons:Add(pistol);
           --p.weapons:Add(rpg);             
           --camera:SetActive(true);
           --p:RefreshWeapons();
                      
           if (isLocal) then
             resMgr:LoadPrefab("ui", {"2DCanvas"}, function(canvas)      
               local cross = UnityEngine.Object.Instantiate(canvas[0]); 
               cross:SetActive(true);
               
               local mainCtrl = CtrlManager.GetCtrl(CtrlNames.Main);
               mainCtrl:Awake();
        
               --onEnter();           
             end)
           end
           --resMgr:UnloadAssetBundle("weapons", false);
           --resMgr:UnloadAssetBundle("player", false);
           --resMgr:UnloadAssetBundle("scenes", false);
           
           --MessageCtrl.Close();
           --Util.ClearMemory();
        --end)
      end)
end



