
Player = {};

function Player.Init(player)
  print('Player.Init');
  
  resMgr:LoadPrefab("weapons", {"Pistol 1911", "RPG"}, function(weapons)      
    local pistol = UnityEngine.Object.Instantiate(weapons[0]); 
    local rpg = UnityEngine.Object.Instantiate(weapons[1]); 
    
     local p = player:GetComponent('PlayerAPI');
     p:AddWeapon(pistol);
     p:AddWeapon(rpg);             
     --camera:SetActive(true);
     p:RefreshWeapons();
  end)
end

