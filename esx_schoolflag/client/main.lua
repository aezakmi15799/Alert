local UIManagerClass = require('client/classes/UIManager')
local FlagObjectClass = require('client/classes/FlagObject')
local PlayerClientClass = require('client/classes/PlayerClient')

UIManager = UIManagerClass:new()
PlayerClient = PlayerClientClass:new()

RegisterNetEvent('schoolflag:updateUI')
AddEventHandler('schoolflag:updateUI', function(data)
    if not UIManager.visible then
        UIManager:show(data)
    else
        UIManager:update(data)
    end
end)

-- Example key mapping
RegisterCommand('pickupflag', function()
    PlayerClient:pickup('school_a')
end)
