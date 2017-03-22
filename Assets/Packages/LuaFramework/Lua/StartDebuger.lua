--ZBS = "D:/wd/trunk/tools/ZeroBraneStudioEduPack/";  
ZBS = "../tools/ZeroBraneStudioEduPack/";  
LuaPath = "Assets/Packages/LuaFramework/Lua/"  
--package.path = package.path..";./?.lua;"..ZBS.."lualibs/?/?.lua;"..ZBS.."lualibs/?.lua;"..LuaPath.."?.lua;"  
package.path = package.path..";./?.lua;"..ZBS.."lualibs/?/?.lua;"..ZBS.."lualibs/?.lua;"..LuaPath.."?.lua;"
require("mobdebug").start()




