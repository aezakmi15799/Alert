local Team = {}
Team.__index = Team

function Team:new(name)
    local self = setmetatable({}, Team)
    self.name = name
    self.score = 0
    return self
end

function Team:addScore(amount)
    self.score = self.score + amount
end

return Team
