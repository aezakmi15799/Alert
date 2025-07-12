local Player = {}
Player.__index = Player

function Player:new(source, data)
    local self = setmetatable({}, Player)
    self.source = source
    self.identifier = data.identifier
    self.job = data.job
    self.score = 0
    return self
end

function Player:addScore(amount)
    self.score = self.score + amount
end

return Player
