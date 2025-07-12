local GameManager = {}
GameManager.__index = GameManager

function GameManager:new()
    local self = setmetatable({}, GameManager)
    self.players = {}
    self.teams = {}
    self.currentRound = 0
    self.timer = nil
    return self
end

function GameManager:startGame()
    -- start round logic
    self.currentRound = self.currentRound + 1
    if self.timer then
        self.timer:stop()
    end
    self.timer = Timer:new(Config.GameDuration * 60, function()
        self:finishRound()
    end)
    self.timer:start()
    self:syncUI()
end

function GameManager:finishRound()
    self:syncUI(true)
end

function GameManager:addPlayer(source, data)
    local player = Player:new(source, data)
    self.players[source] = player
    return player
end

function GameManager:getTeam(job)
    return self.teams[job]
end

function GameManager:syncUI(finish)
    TriggerClientEvent('schoolflag:updateUI', -1, {
        players = self.players,
        round = self.currentRound,
        finish = finish or false
    })
end

return GameManager
