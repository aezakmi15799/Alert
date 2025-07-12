local FlagObject = {}
FlagObject.__index = FlagObject

function FlagObject:new(coords, team)
    local self = setmetatable({}, FlagObject)
    self.coords = coords
    self.team = team
    self.entity = nil
    return self
end

function FlagObject:spawn()
    if not self.entity then
        RequestModel('prop_flagpole_2b')
        while not HasModelLoaded('prop_flagpole_2b') do
            Citizen.Wait(0)
        end
        self.entity = CreateObject('prop_flagpole_2b', self.coords.x, self.coords.y, self.coords.z, false, false, false)
    end
end

function FlagObject:delete()
    if self.entity then
        DeleteEntity(self.entity)
        self.entity = nil
    end
end

return FlagObject
