local PlayerClient = {}
PlayerClient.__index = PlayerClient

function PlayerClient:new()
    local self = setmetatable({}, PlayerClient)
    self.flag = nil
    return self
end

function PlayerClient:pickup(team)
    TriggerServerEvent('schoolflag:pickupFlag', team)
end

return PlayerClient
