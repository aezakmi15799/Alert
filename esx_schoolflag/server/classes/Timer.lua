local Timer = {}
Timer.__index = Timer

function Timer:new(seconds, cb)
    local self = setmetatable({}, Timer)
    self.duration = seconds
    self.callback = cb
    self.running = false
    return self
end

function Timer:start()
    self.running = true
    Citizen.CreateThread(function()
        local start = os.time()
        while self.running and os.difftime(os.time(), start) < self.duration do
            Citizen.Wait(1000)
        end
        if self.running then
            self.callback()
        end
    end)
end

function Timer:stop()
    self.running = false
end

return Timer
